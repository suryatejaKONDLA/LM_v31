import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import Root from "./Root";
import "antd/dist/reset.css";
import "./global.css";

const rootElement = document.getElementById("root");

if (!rootElement)
{
    throw new Error("Root element not found");
}

createRoot(rootElement).render(
    <StrictMode>
        <Root />
    </StrictMode>,
);
