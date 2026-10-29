const { env } = require('process');

const notifications = env["services__notification-services__http__0"]
  ?? env["services__notification-services__https__0"]
  ?? 'https://localhost:5300';

const inventory = env["services__inventory-services__http__0"]
  ?? env["services__inventory-services__https__0"]
  ?? 'https://localhost:5307';

const operatives = env["services__operative-services__http__0"]
  ?? env["services__operative-services__https__0"]
  ?? 'https://localhost:5308';

const keycloak = env["services__mountypython-keycloak__http__0"]
  ?? env["services__mountypython-keycloak__https__0"]
  ?? 'http://localhost:9001';

const PROXY_CONFIG = [
  {
    context: ["/auth"],
    target: keycloak,
    secure: false,
    changeOrigin: true,
    pathRewrite: { "^/auth": "" }
  },
  {
    context: ["/resources", "/realms", "/js", "/themes"],
    target: keycloak,
    secure: false,
    changeOrigin: true
  },
  {
    context: ["/api/notifications", "/api/v1/updateshub"],
    target: notifications,
    secure: false,
    changeOrigin: true,
    ws: true
  },
  {
    context: ["/api/catalogs",],
    target: inventory,
    secure: false,
    changeOrigin: true
  },
  {
    context: ["/api/operations",],
    target: operatives,
    secure: false,
    changeOrigin: true
  }
]

module.exports = PROXY_CONFIG;
