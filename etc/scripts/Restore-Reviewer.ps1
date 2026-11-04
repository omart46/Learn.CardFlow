$ErrorActionPreference = 'Stop'
$cardFlowRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Push-Location $cardFlowRoot
try {
    # Read only the old reviewer, keeping password hashes out of logs and files.
    $legacyJson = node -e 'const {DatabaseSync}=require("node:sqlite"); const d=new DatabaseSync("CardFlow.db",{readOnly:true}); const user=d.prepare("SELECT * FROM AbpUsers WHERE lower(UserName)=? AND IsDeleted=0").get("reviewer"); const role=d.prepare("SELECT * FROM AbpRoles WHERE Name=?").get("Revieswer"); if(!user||!role)process.exit(1); console.log(JSON.stringify({user,role}));'
    if ($LASTEXITCODE -ne 0) { throw 'The legacy reviewer was not found.' }
    $legacy = $legacyJson | ConvertFrom-Json
    $settings = Get-Content 'src/Learn.CardFlow.DbMigrator/appsettings.secrets.json' -Raw | ConvertFrom-Json
    $sql = [System.Data.SqlClient.SqlConnection]::new($settings.ConnectionStrings.Default)
    $sql.Open()
    $transaction = $sql.BeginTransaction()
    try {
        function Invoke-ReviewerScalar($query, $parameters) {
            $cmd = $sql.CreateCommand()
            $cmd.Transaction = $transaction
            $cmd.CommandText = $query
            foreach ($key in $parameters.Keys) { [void]$cmd.Parameters.AddWithValue($key, $parameters[$key]) }
            return $cmd.ExecuteScalar()
        }

        function Insert-ReviewerRow($table, $row) {
            $schemaCommand = $sql.CreateCommand()
            $schemaCommand.Transaction = $transaction
            $schemaCommand.CommandText = "SELECT TOP (0) * FROM [$table]"
            $reader = $schemaCommand.ExecuteReader()
            $schema = $reader.GetSchemaTable()
            $reader.Close()
            $insert = $sql.CreateCommand()
            $insert.Transaction = $transaction
            $columns = @()
            $values = @()
            foreach ($column in $schema.Rows) {
                $property = $row.PSObject.Properties[$column.ColumnName]
                if ($null -eq $property -or $column.IsIdentity) { continue }
                $value = $property.Value
                $parameterName = '@p' + $columns.Count
                $columns += '[' + $column.ColumnName + ']'
                $values += $parameterName
                if ($null -eq $value) {
                    $value = [DBNull]::Value
                } elseif ($column.DataType -eq [Guid]) {
                    $value = [Guid]::Parse([string]$value)
                } elseif ($column.DataType -eq [DateTime]) {
                    $value = [DateTime]::Parse([string]$value, [Globalization.CultureInfo]::InvariantCulture)
                } elseif ($column.DataType -eq [bool]) {
                    $value = [bool][int]$value
                } elseif ($column.DataType -eq [int]) {
                    $value = [int]$value
                } elseif ($column.DataType -eq [long]) {
                    $value = [long]$value
                }
                [void]$insert.Parameters.AddWithValue($parameterName, $value)
            }
            $insert.CommandText = "INSERT INTO [$table] (" + ($columns -join ',') + ') VALUES (' + ($values -join ',') + ')'
            [void]$insert.ExecuteNonQuery()
        }

        $roleId = Invoke-ReviewerScalar 'SELECT Id FROM AbpRoles WHERE NormalizedName=@name' @{ '@name' = 'REVIEWER' }
        if ($null -eq $roleId) {
            $legacy.role.Name = 'Reviewer'
            $legacy.role.NormalizedName = 'REVIEWER'
            $legacy.role.IsDefault = 0
            Insert-ReviewerRow 'AbpRoles' $legacy.role
            $roleId = [Guid]$legacy.role.Id
        }
        $userId = Invoke-ReviewerScalar 'SELECT Id FROM AbpUsers WHERE NormalizedUserName=@name AND IsDeleted=0' @{ '@name' = 'REVIEWER' }
        if ($null -eq $userId) {
            $legacy.user.IsActive = 1
            $legacy.user.AccessFailedCount = 0
            $legacy.user.LockoutEnd = $null
            Insert-ReviewerRow 'AbpUsers' $legacy.user
            $userId = [Guid]$legacy.user.Id
        }
        [void](Invoke-ReviewerScalar 'IF NOT EXISTS (SELECT 1 FROM AbpUserRoles WHERE UserId=@user AND RoleId=@role) INSERT INTO AbpUserRoles (UserId,RoleId,TenantId) VALUES (@user,@role,NULL)' @{ '@user' = $userId; '@role' = $roleId })
        foreach ($permission in @('CardFlow.CardRequests', 'CardFlow.CardRequests.StartReview')) {
            [void](Invoke-ReviewerScalar "IF NOT EXISTS (SELECT 1 FROM AbpPermissionGrants WHERE ProviderName='R' AND ProviderKey=@role AND Name=@name) INSERT INTO AbpPermissionGrants (Id,TenantId,Name,ProviderName,ProviderKey) VALUES (@id,NULL,@name,'R',@role)" @{ '@id' = [Guid]::NewGuid(); '@role' = 'Reviewer'; '@name' = $permission })
        }
        $transaction.Commit()
        Write-Output 'Reviewer restored with the existing password and the view/start-review permissions.'
    } catch {
        $transaction.Rollback()
        throw
    } finally {
        $sql.Dispose()
    }
} finally {
    Pop-Location
}
