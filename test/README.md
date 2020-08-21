# Simple example with a provider written in C#

This provider is implemented in the `terraform-provider-dotnetsample` project and simply adds the `Echo: `-prefix to a string and provides the result as an output value.

To run the sample you have to run the provider as administrator first and make sure group policy to allow root certification installation is enabled (see global Readme).