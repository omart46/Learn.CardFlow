import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44367/',
  redirectUri: baseUrl,
  clientId: 'CardFlow_App',
  responseType: 'code',
  scope: 'offline_access CardFlow',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'CardFlow',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44367',
      rootNamespace: 'Learn.CardFlow',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
