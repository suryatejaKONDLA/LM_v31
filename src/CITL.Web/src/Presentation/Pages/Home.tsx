import { Typography } from "antd";

const { Title, Paragraph } = Typography;

/**
 * Home dashboard page — displayed after successful login.
 */
export default function Home(): React.JSX.Element
{
    return (
        <div>
            <Title level={2}>Dashboard</Title>
            <Paragraph>Welcome to CITL.</Paragraph>
        </div>
    );
}
