
# Define Terraform Plugins in .NET

Similar to the [GO SDK Project](https://github.com/hashicorp/terraform-plugin-sdk) but for .NET

## Interface (GPRC)

See [Readme in the protobuf folder](./terraform-pugin-sdk-dotnet/protobuf/Readme.md).

## Current State of the project

- Working POC (no usable library yet)
- TLS Handshake, currently "Allow user trusted root CAs to be validate certificates" needs to be enabled in group policy under
  "Computer Configuration\Windows Settings\Security Settings\Public Key Policies\Certificate Path Validation Settings option\Stores"
  see https://serverfault.com/questions/1008156/how-can-you-certificate-path-validation-settings-programatically-or-via-intune

## WHY?

Allow extensions of Terraform written in .NET languages (C#, F#).
The goal is to have a similar API to [the official docs](https://learn.hashicorp.com/tutorials/terraform/provider-setup).
This allows us to reuse existing docs and to write providers and provisioners in C#.

## Changes to official APIs

- Some deprecated members are missing

