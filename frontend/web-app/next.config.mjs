/** @type {import('next').NextConfig} */
const nextConfig = {
    experimental: {
        serverActions: true
    },
    images: {
        domains: [
            'cdn.pixabay.com',
            'media.istockphoto.com'
        ]
    },
    output: 'standalone'
};

export default nextConfig;
