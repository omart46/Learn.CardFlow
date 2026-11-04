using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Learn.CardFlow.CardRequests;

public class EmployeeCardRequestManager : DomainService
{
    public EmployeeCardRequest Create(
        string employeeName,
        string employeeNumber)
    {
        employeeName = Check.NotNullOrWhiteSpace(
            employeeName,
            nameof(employeeName),
            maxLength: 128
        );

        employeeNumber = Check.NotNullOrWhiteSpace(
            employeeNumber,
            nameof(employeeNumber),
            maxLength: 32
        );

        return new EmployeeCardRequest(
            GuidGenerator.Create(),
            employeeName,
            employeeNumber
        );
    }
}
