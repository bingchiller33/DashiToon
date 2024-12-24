import React from "react";
import Header from "./Header";
import Footer from "./Footer";
import BackToTopButton from "./BackToTopButton";
import PermissionChecker, { PermissionCheckerProps } from "./PermissionChecker";
export interface SiteLayoutProps extends PermissionCheckerProps {
    children: React.ReactNode;
    extras?: React.ReactNode;
}

export default function SiteLayout(props: SiteLayoutProps) {
    const { children, extras } = props;
    return (
        <div className="flex min-h-screen flex-col bg-gradient-to-b from-neutral-900 to-neutral-800">
            <Header />
            <main className="flex flex-grow flex-col">
                <PermissionChecker {...props}>
                    {children}
                    <BackToTopButton />
                </PermissionChecker>
            </main>
            <Footer />
            {extras}
        </div>
    );
}
