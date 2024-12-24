"use client";

import { useState, useEffect } from "react";
import { ChevronUp } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function BackToTopButton() {
    const [isVisible, setIsVisible] = useState(false);

    useEffect(() => {
        const toggleVisibility = () => {
            // Show button when page is scrolled down 300px
            if (window.scrollY > 300) {
                setIsVisible(true);
            } else {
                setIsVisible(false);
            }
        };

        window.addEventListener("scroll", toggleVisibility);

        return () => window.removeEventListener("scroll", toggleVisibility);
    }, []);

    const scrollToTop = () => {
        window.scrollTo({
            top: 0,
            behavior: "smooth",
        });
    };

    return (
        <>
            {isVisible && (
                <Button
                    className="fixed bottom-4 right-4 z-50 rounded-full p-2 shadow-lg transition-opacity duration-300 hover:opacity-80 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2"
                    onClick={scrollToTop}
                    aria-label="Back to top"
                >
                    <ChevronUp className="h-6 w-6" />
                    <span className="sr-only">Back to top</span>
                </Button>
            )}
        </>
    );
}
