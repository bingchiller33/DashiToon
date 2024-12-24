import { useState } from "react";

export function useContentDialog() {
    const [isOpen, setIsOpen] = useState(false);

    return {
        isOpen,
        onOpen: () => setIsOpen(true),
        onClose: () => setIsOpen(false),
    };
}
