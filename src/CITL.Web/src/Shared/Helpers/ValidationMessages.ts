/**
 * Validation messages and patterns for react-hook-form rules.
 * Centralizes all validation constants used across form controls.
 */
export const V = {
    Required: "This field is Required",
    Email: "Invalid Email",
    EmailPattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
    Phone: "Invalid Phone Number",
    PhoneNumberPattern: /^[6-9]\d{9}$/,
    MaxLength: (max: number): string => `Max Length ${max.toString()}`,
    MinLength: (min: number): string => `Min Length ${min.toString()}`,
    StringLength: (min: number, max: number): string => `Length Between ${min.toString()} - ${max.toString()}`,
    PasswordMatch: "Password doesn't match",
    NoSpaceAndNoSymbols: "Spaces and Symbols Not Allowed",
    NoSpaceAndNoSymbolsPattern: /^[a-zA-Z0-9]+$/,
    NumericOnly: "Only numbers are allowed",
} as const;
