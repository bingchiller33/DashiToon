import { format, set } from "date-fns";
import { vi } from "date-fns/locale";
import { DateRange } from "react-day-picker";

export const validateEmail = (email: string) => {
    return String(email)
        .toLowerCase()
        .match(
            /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|.(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/,
        );
};

export const validatePassword = (password: string) => {
    return String(password).match(/^.{6,100}$/);
};

export async function openFilePicker(
    accept: string = "image/jpeg, image/png",
    multiple: boolean = true,
): Promise<File[] | null> {
    return new Promise((resolve, reject) => {
        const fileInput = document.createElement("input");
        fileInput.type = "file";
        fileInput.accept = accept;
        fileInput.multiple = multiple;
        // Trigger the file picker dialog
        fileInput.click();

        // Listen for the file selection
        fileInput.addEventListener("change", (e) => {
            const files = (e?.target as HTMLInputElement).files;
            resolve(files ? Array.from(files) : null);
        });
        fileInput.addEventListener("cancel", () => resolve(null));
    });
}

export function getPagination(
    currentPage: number,
    totalPages: number,
    delta: number = 1,
) {
    const pages = [] as (number | string)[];

    if (currentPage >= 1) {
        pages.push(1);
    }

    if (currentPage > delta + 2) {
        pages.push("...");
    }

    for (
        let i = Math.max(2, currentPage - delta);
        i <= Math.min(totalPages - 1, currentPage + delta);
        i++
    ) {
        pages.push(i);
    }

    if (currentPage < totalPages - delta - 1) {
        pages.push("...");
    }

    if (currentPage < totalPages) {
        pages.push(totalPages);
    }

    return pages;
}

export function isFullScreen() {
    try {
        return screen.height - 30 <= window.innerHeight;
    } catch (e) {
        return false;
    }
}

export async function toggleFullScreen() {
    try {
        if (isFullScreen()) {
            await document.exitFullscreen();
        } else {
            await document.documentElement.requestFullscreen();
        }
    } catch (e) {
        console.error(e);
    }
}

export function formatCurrency(amount: number) {
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
    }).format(amount);
}

export function formatDateTime(date: Date | string) {
    return format(new Date(date), "dd/MM/yyyy HH:mm", { locale: vi });
}

export function delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(() => resolve(), ms));
}

export function formatDateRange(d: DateRange) {
    const from = d.from?.toLocaleDateString();
    const to = d.to?.toLocaleDateString();
    return `${from} - ${to}`;
}
