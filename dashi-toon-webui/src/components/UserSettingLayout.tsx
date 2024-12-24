"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { cn } from "@/lib/utils";

import { Separator } from "./ui/separator";
import { buttonVariants } from "./ui/button";
import {
    Bell,
    Brush,
    CalendarClock,
    CircleUser,
    History,
    IdCard,
} from "lucide-react";

const sidebarNavItems = [
    {
        title: "Hồ sơ",
        href: "/examples/forms",
        icon: <IdCard />,
    },
    {
        title: "Tài khoản",
        href: "/examples/forms/account",
        icon: <CircleUser />,
    },
    {
        title: "Đăng ký DashiFan",
        href: "/account/subscriptions",
        icon: <CalendarClock />,
    },
    {
        title: "Lịch sử thanh toán",
        href: "/account/payment-history",
        icon: <History />,
    },
    {
        title: "Giao diện",
        href: "/examples/forms/appearance",
        icon: <Brush />,
    },
    {
        title: "Thông báo",
        href: "/examples/forms/notifications",
        icon: <Bell />,
    },
];

export default function UserSettingLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    return (
        <div className="space-y-6 pb-16">
            <div className="space-y-0.5">
                <h2 className="text-2xl font-bold tracking-tight">Cài đặt</h2>
                <p className="text-muted-foreground">
                    Quản lý cài đặt tài khoản của bạn trên DashiToon!
                </p>
            </div>
            <Separator className="my-6" />
            <div className="flex flex-col space-y-8 lg:flex-row lg:space-x-12 lg:space-y-0">
                <aside className="-mx-4 lg:w-1/5">
                    <SidebarNav items={sidebarNavItems} />
                </aside>
                <div className="w-full flex-grow">{children}</div>
            </div>
        </div>
    );
}

interface SidebarNavProps extends React.HTMLAttributes<HTMLElement> {
    items: {
        href: string;
        title: string;
        icon: React.ReactNode;
    }[];
}

export function SidebarNav({ className, items, ...props }: SidebarNavProps) {
    const pathname = usePathname();

    return (
        <nav
            className={cn(
                "flex flex-col gap-2 lg:space-x-0 lg:space-y-1",
                className,
            )}
            {...props}
        >
            {items.map((item) => (
                <Link
                    key={item.href}
                    href={item.href}
                    className={cn(
                        buttonVariants({ variant: "ghost" }),
                        pathname === item.href
                            ? "bg-muted hover:bg-muted"
                            : "hover:bg-transparent hover:underline",
                        "justify-start gap-2",
                    )}
                >
                    {item.icon}
                    {item.title}
                </Link>
            ))}
        </nav>
    );
}
