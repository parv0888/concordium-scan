terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
      version = "=2.97.0"
    }
  }
}

provider "azurerm" {
  features {}
}

