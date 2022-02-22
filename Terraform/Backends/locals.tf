locals {
  environment = terraform.workspace
  envs = {
    "dev": {
        resource_group_name = "ccscan-env-dev"
        azure_vm_size = "Standard_D8s_v4"
        container_repository_backend = "backend.dev"
        domain_name = "dev-api.ccdscan.io"
    }
    "test": {
        resource_group_name = "ccscan-env-test"
        azure_vm_size = "Standard_D8s_v4"
        container_repository_backend = "backend.test"
        domain_name = "test-api.ccdscan.io"
    }
  }

  vm_admin_user = "concNodeVMuser"

  vm_entrypoint_script = templatefile("${path.cwd}/content/entrypoint.tpl", {
    vm_user = local.vm_admin_user
    environment_name = local.environment
    container_repository_backend = local.envs[local.environment].container_repository_backend
    container_registry_username = data.azurerm_container_registry.ccscan.admin_username
    container_registry_password = data.azurerm_container_registry.ccscan.admin_password 
    postgres_user = "postgres"
    postgres_password = data.azurerm_key_vault_secret.postgres-password.value
    cc_node_auth_token = data.azurerm_key_vault_secret.ccnode-auth-token.value
  })
}
