import { readFileSync, writeFileSync } from "node:fs";

const pkgPath = "./package.json";
const pkg = JSON.parse(readFileSync(pkgPath, "utf-8"));

const now = new Date();
const pad = (n) => String(n).padStart(2, "0");

const version = [
    pad(now.getDate()),
    pad(now.getMonth() + 1),
    now.getFullYear(),
    pad(now.getHours()) + pad(now.getMinutes()),
].join(".");

pkg.version = version;

writeFileSync(pkgPath, JSON.stringify(pkg, null, 2) + "\n");

console.log(`Version updated to ${version}`);
