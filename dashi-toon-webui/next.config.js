/** @type {import('next').NextConfig} */
const nextConfig = {
    output: "standalone",
    images: {
        remotePatterns: [
            {
                protocol: "https",
                hostname: "dashitoon.s3.ap-southeast-1.amazonaws.com",
                port: "",
                pathname: "/thumbnails/**",
            },
            {
                protocol: "https",
                hostname: "**",
            },
            {
                protocol: "https",
                hostname: "www.paypal.com",
                port: "",
                pathname: "/**",
            },
            {
                protocol: "https",
                hostname: "www.sandbox.paypal.com",
                port: "",
                pathname: "/**",
            },
        ],
    },
    async headers() {
        return [
            {
                source: "/:path*",
                headers: [
                    {
                        key: "Content-Security-Policy",
                        value: "frame-ancestors 'self' https://*.paypal.com",
                    },
                    {
                        key: "Access-Control-Allow-Origin",
                        value: "*",
                    },
                    {
                        key: "Access-Control-Allow-Headers",
                        value: "*",
                    },
                    {
                        key: "Access-Control-Allow-Methods",
                        value: "*",
                    },
                ],
            },
        ];
    },
};

module.exports = nextConfig;
