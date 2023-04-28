# SimplePassive<br>[![GitHub Actions][actions-img]][actions-url] [![Patreon][patreon-img]][patreon-url] [![PayPal][paypal-img]][paypal-url] [![Discord][discord-img]][discord-url]

SimplePassive is a resource for FiveM that allows you to use a Passive Mode like feature just like in GTA Online.

It has multiple configurable features that you can configure and use, like:

* Authorizing specific players via ACL permissions (for example: only staff members can use passive)
* Forcing passive mode onto players (to prevent harassment or ramming with vehicles)
* Disable combat between players (for the true passive mode experience)

## Download

* [GitHub Releases](https://github.com/justalemon/SimplePassive/releases)
* [GitHub Actions](https://github.com/justalemon/SimplePassive/actions) (experimental versions)

## Installation

In your **resources** directory, create a folder called **simplepassive** and extracts the contents from the compressed file in there.

## Usage

By default, SimplePassive does not allows users to enable or disable passive mode. You need to authorize it via the `simplepassive.changeself` ACL permission (you can allow everyone with `add_ace builtin.everyone simplepassive.changeself allow`). Then, passive mode can be toggled via the /passivetoggle command.

Feel free to check out the [wiki](https://github.com/justalemon/SimplePassive/wiki) for a list of Commands, Convars and Exports.

[actions-img]: https://img.shields.io/github/actions/workflow/status/justalemon/SimplePassive/main.yml?branch=master&label=actions
[actions-url]: https://github.com/justalemon/SimplePassive/actions
[patreon-img]: https://img.shields.io/badge/support-patreon-FF424D.svg
[patreon-url]: https://www.patreon.com/lemonchan
[paypal-img]: https://img.shields.io/badge/support-paypal-0079C1.svg
[paypal-url]: https://paypal.me/justalemon
[discord-img]: https://img.shields.io/badge/discord-join-7289DA.svg
[discord-url]: https://discord.gg/Cf6sspj
