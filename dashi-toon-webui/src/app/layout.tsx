import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import SignalRProvider from "@/components/SignalRProvider";
import { Toaster } from "sonner";
import { ThemeProvider } from "@/components/ThemeProvider";
import { TooltipProvider } from "@/components/ui/tooltip";
import UwUProvider from "@/components/UwUProvider";
import QCProvider from "@/components/QCProvider";
import Gtag from "@/components/Gtag";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
    title: "DashiToon",
    description: "Nền tảng đọc truyện trực tuyến hàng đầu Việt Nam.",
};

export default function RootLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    return (
        <html lang="en">
            <head>
                <Gtag />
            </head>
            <SignalRProvider>
                <body className={inter.className}>
                    <ThemeProvider
                        attribute="class"
                        defaultTheme="dark"
                        enableSystem
                        disableTransitionOnChange
                    >
                        <QCProvider>
                            <UwUProvider>
                                <TooltipProvider>{children}</TooltipProvider>
                                <Toaster richColors />
                            </UwUProvider>
                        </QCProvider>
                    </ThemeProvider>
                </body>
            </SignalRProvider>
        </html>
    );
}
