"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { cn } from "@/lib/utils";

import { Separator } from "./ui/separator";
import { buttonVariants } from "./ui/button";
import {
    ArrowLeftRight,
    Bell,
    Brush,
    CalendarClock,
    ChartColumn,
    CircleUser,
    Coins,
    History,
    IdCard,
} from "lucide-react";

const sidebarNavItems = [
    {
        title: "Quản lý Kana",
        href: "/admin/kana",
        icon: <Coins />,
    },
    {
        title: "Quản lý DashiFan",
        href: "/admin/dashifan",
        icon: <CalendarClock />,
    },
    {
        title: "Thống kê hệ thống",
        href: "/admin/analytics",
        icon: <ChartColumn />,
    },
    {
        title: "Quản lý tài khoản",
        href: "/admin/users",
        icon: <CircleUser />,
    },
    {
        title: "Quản lý thu chi",
        href: "/admin/transactions",
        icon: <ArrowLeftRight />,
    },
];

export default function AdminSettingLayout({ children }: { children: React.ReactNode }) {
    return (
        <div className="space-y-6 pb-16">
            <div className="space-y-0.5">
                <h2 className="text-2xl font-bold tracking-tight">Cài đặt hệ thống</h2>
                <p className="text-muted-foreground">Quản lý cài đặt hệ thống DashiToon cho quản trị viên!</p>
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
        <nav className={cn("flex flex-col gap-2 lg:space-x-0 lg:space-y-1", className)} {...props}>
            {items.map((item) => (
                <Link
                    key={item.href}
                    href={item.href}
                    className={cn(
                        buttonVariants({ variant: "ghost" }),
                        pathname === item.href ? "bg-muted hover:bg-muted" : "hover:bg-transparent hover:underline",
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
