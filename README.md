# Drexel.Host
Miscellaneous junk for my server

# Installation
## Linux
* `sudo setcap cap_sys_boot+ep /path/to/your/executable`
  * required to access the `reboot` syscall (via the `reboot` libc wrapper function)