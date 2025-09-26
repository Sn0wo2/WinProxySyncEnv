# 🌐 WinProxySyncEnv

> **Automatically sync Windows system proxy settings to environment variables**
> Solves the issue where some programs (e.g., `git`) do not use the Windows system proxy.
> This tool supports **startup on boot** and **real-time synchronization**, eliminating the need for manual environment variable configuration.

[![GitHub release](https://img.shields.io/github/v/release/Sn0wo2/WinProxySyncEnv?color=blue)](https://github.com/Sn0wo2/WinProxySyncEnv/releases)
[![GitHub License](https://img.shields.io/github/license/Sn0wo2/WinProxySyncEnv)](LICENSE)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FSn0wo2%2FWinProxySyncEnv.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FSn0wo2%2FWinProxySyncEnv?ref=badge_shield)

[![Automatic Dependency Submission](https://github.com/Sn0wo2/WinProxySyncEnv/actions/workflows/dependency-graph/auto-submission/badge.svg)](https://github.com/Sn0wo2/WinProxySyncEnv/actions/workflows/dependency-graph/auto-submission)
[![Dependabot Updates](https://github.com/Sn0wo2/WinProxySyncEnv/actions/workflows/dependabot/dependabot-updates/badge.svg)](https://github.com/Sn0wo2/WinProxySyncEnv/actions/workflows/dependabot/dependabot-updates)
[![CodeQL Advanced](https://github.com/Sn0wo2/WinProxySyncEnv/actions/workflows/codeql.yml/badge.svg)](https://github.com/Sn0wo2/WinProxySyncEnv/actions/workflows/codeql.yml)
---

## 🚀 Features

* ✅ Automatically syncs **system proxy** to **environment variables**
* ✅ Supports **auto start on boot**
* ✅ Supports **real-time synchronization** when proxy settings change

---

## ⚡ Usage

### **Install and Run**

```cmd
.\WinProxySyncEnv install
.\WinProxySyncEnv
```

### **Uninstall**

```cmd
.\WinProxySyncEnv uninstall
```

---

## 📌 Use Cases

* Programs like `git` that rely on proxy environment variables
* Seamless proxy usage in command-line tools or other applications

## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FSn0wo2%2FWinProxySyncEnv.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FSn0wo2%2FWinProxySyncEnv?ref=badge_large)