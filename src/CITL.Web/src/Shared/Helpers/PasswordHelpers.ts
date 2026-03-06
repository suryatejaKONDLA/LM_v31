/**
 * Password generation, strength calculation, and validation patterns.
 */

export interface PasswordStrengthResult
{
    score: number;
    label: string;
    color: string;
}

const Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
const Lowercase = "abcdefghijklmnopqrstuvwxyz";
const Digits = "0123456789";
const Specials = "@$!%*?&";
const AllChars = Uppercase + Lowercase + Digits + Specials;

/** Minimum required password length. */
export const PasswordMinLength = 8;

/** Regex requiring at least one uppercase, one lowercase, one digit, and one special character. */
export const PasswordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$/;

/** Human-readable message for PasswordPattern failures. */
export const PasswordPatternMessage = "Password must contain uppercase, lowercase, number and special character";

/**
 * Generate a cryptographically random strong password.
 * Guarantees at least one of each required character class.
 */
export function generateStrongPassword(length = 16): string
{
    const randomIndex = (max: number): number =>
    {
        const array = new Uint32Array(1);
        crypto.getRandomValues(array);
        return (array[0] ?? 0) % max;
    };

    const randomChar = (chars: string): string =>
        chars.charAt(randomIndex(chars.length));

    const required = [
        randomChar(Uppercase),
        randomChar(Lowercase),
        randomChar(Digits),
        randomChar(Specials),
    ];

    for (let i = required.length; i < length; i++)
    {
        required.push(randomChar(AllChars));
    }

    // Fisher-Yates shuffle
    for (let i = required.length - 1; i > 0; i--)
    {
        const j = randomIndex(i + 1);
        const temp = required[i];
        required[i] = required[j] ?? "";
        required[j] = temp ?? "";
    }

    return required.join("");
}

/**
 * Calculate password strength on a 0–100 scale.
 * Scores based on length thresholds and character class diversity.
 */
export function calculatePasswordStrength(password: string): PasswordStrengthResult
{
    let score = 0;

    if (password.length >= 8)
    {
        score += 25;
    }
    if (password.length >= 12)
    {
        score += 15;
    }
    if (password.length >= 16)
    {
        score += 10;
    }
    if (/[a-z]/.test(password))
    {
        score += 10;
    }
    if (/[A-Z]/.test(password))
    {
        score += 10;
    }
    if (/\d/.test(password))
    {
        score += 15;
    }
    if (/[@$!%*?&]/.test(password))
    {
        score += 15;
    }

    if (score <= 25)
    {
        return { score, label: "Weak", color: "#ff4d4f" };
    }
    if (score <= 50)
    {
        return { score, label: "Fair", color: "#faad14" };
    }
    if (score <= 75)
    {
        return { score, label: "Good", color: "#52c41a" };
    }
    return { score, label: "Strong", color: "#1890ff" };
}
