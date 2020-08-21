

provider "dotnetsample" {
}

resource "dotnetsample_test" "main" {
  input     = var.input
}

