import * as React from "react";
import Link from "next/link";
import { Book, LogOut, Menu, PenTool, Search, Settings, Shield, User } from "lucide-react";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet";
import Image from "next/image";
import { Avatar, AvatarFallback, AvatarImage } from "./ui/avatar";
import { useEffect, useState } from "react";
import { getUserInfo, logoutUser, UserSession } from "@/utils/api/user";
import SearchBar from "./SearchBar";
import KanaCoinDropDown from "./KanaCoinDropDown";
import { ROLES } from "@/utils/consts";
import { useLocalStorage } from "usehooks-ts";
import NotificationsPopover from "./Notification";

export default function Header() {
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const [session, setSession] = useLocalStorage<UserSession | null>("session", null);
    useEffect(() => {
        async function work() {
            const [s, e] = await getUserInfo();
            if (e) {
                setSession(null);
                return;
            }

            setSession(s);
        }

        work();
    }, [setSession]);

    async function handleLogout() {
        logoutUser();
        setSession(null);
    }

    return (
        <header className="sticky top-0 z-30 border-b border-neutral-700 bg-neutral-900 shadow-lg shadow-black/50 backdrop-blur-sm">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex h-16 items-center justify-between">
                    <div className="flex items-center">
                        <Link href="/" className="flex items-center text-blue-400">
                            <Image src="/logo.svg" width={32} height={32} alt="logo" />
                            <span className="ml-2 hidden text-xl font-bold text-white lg:block">DashiToon</span>
                        </Link>
                        <nav className="hidden md:ml-6 md:flex md:space-x-4">
                            <Link
                                href="/search"
                                className="rounded-md px-3 py-2 text-sm font-medium text-gray-300 hover:text-blue-400"
                            >
                                Khám phá
                            </Link>
                            <Link
                                href="/following"
                                className="rounded-md px-3 py-2 text-sm font-medium text-gray-300 hover:text-blue-400"
                            >
                                Đang theo dõi
                            </Link>
                            <Link
                                href="/recent-updates"
                                className="rounded-md px-3 py-2 text-sm font-medium text-gray-300 hover:text-blue-400"
                            >
                                Vừa cập nhật
                            </Link>
                        </nav>
                    </div>
                    <div className="flex items-center">
                        <div className="">
                            <SearchBar />
                        </div>
                        <div className="hidden md:ml-4 md:block">
                            <KanaCoinDropDown session={session} />
                        </div>
                        <div className="hidden md:ml-2 md:block">
                            <NotificationsPopover session={session} />
                        </div>
                        <div className="hidden md:ml-4 md:flex md:items-center">
                            {session ? (
                                <DropdownMenu>
                                    <DropdownMenuTrigger asChild>
                                        <Button variant="ghost" className="relative h-8 w-8 rounded-full">
                                            <Avatar className="h-8 w-8">
                                                <AvatarImage
                                                    src="/placeholder.svg?height=32&width=32"
                                                    alt={session.username}
                                                />
                                                <AvatarFallback>{session.username.charAt(0)}</AvatarFallback>
                                            </Avatar>
                                        </Button>
                                    </DropdownMenuTrigger>
                                    <DropdownMenuContent className="w-56" align="end" forceMount>
                                        <DropdownMenuLabel className="font-normal">
                                            <div className="flex flex-col space-y-1">
                                                <p className="text-xs leading-none text-muted-foreground">Xin chào</p>
                                                <p className="h-4 overflow-hidden truncate text-ellipsis text-sm font-medium leading-none">
                                                    {session.username}
                                                </p>
                                            </div>
                                        </DropdownMenuLabel>
                                        <DropdownMenuSeparator />
                                        <DropdownMenuItem>
                                            <User className="mr-2 h-4 w-4" />
                                            <span>Hồ sơ</span>
                                        </DropdownMenuItem>
                                        <Link href="/account/subscriptions">
                                            <DropdownMenuItem>
                                                <Settings className="mr-2 h-4 w-4" />
                                                <span>Cài đặt</span>
                                            </DropdownMenuItem>
                                        </Link>
                                        <Link href="/author-studio/series">
                                            <DropdownMenuItem>
                                                <PenTool className="mr-2 h-4 w-4" />
                                                <span>Xưởng truyện</span>
                                            </DropdownMenuItem>
                                        </Link>
                                        <DropdownMenuSeparator />
                                        {session.roles.includes(ROLES.Administrator) && (
                                            <Link href="/admin/kana">
                                                <DropdownMenuItem>
                                                    <Settings className="mr-2 h-4 w-4" />
                                                    <span>Quản trị hệ thống</span>
                                                </DropdownMenuItem>
                                            </Link>
                                        )}
                                        {session.roles.includes(ROLES.Moderator) && (
                                            <Link href="/moderation/comments">
                                                <DropdownMenuItem>
                                                    <Shield className="mr-2 h-4 w-4" />
                                                    <span>Kiểm duyệt nội dung</span>
                                                </DropdownMenuItem>
                                            </Link>
                                        )}
                                        <DropdownMenuSeparator />
                                        <DropdownMenuItem onClick={handleLogout}>
                                            <LogOut className="mr-2 h-4 w-4" />
                                            <span>Đăng xuất</span>
                                        </DropdownMenuItem>
                                    </DropdownMenuContent>
                                </DropdownMenu>
                            ) : (
                                <>
                                    <Link href="/login">
                                        <Button variant="ghost" className="text-gray-300 hover:text-blue-400">
                                            Đăng nhập
                                        </Button>
                                    </Link>
                                    <Link href="/register">
                                        <Button className="ml-3 bg-blue-600 text-white hover:bg-blue-700">
                                            Đăng ký
                                        </Button>
                                    </Link>
                                </>
                            )}
                        </div>
                        <div className="flex md:hidden">
                            <Sheet open={isMenuOpen} onOpenChange={setIsMenuOpen}>
                                <SheetTrigger asChild>
                                    <Button
                                        variant="ghost"
                                        className="inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:text-blue-400 focus:outline-none"
                                    >
                                        <span className="sr-only">Mở menu chính</span>
                                        <Menu className="h-6 w-6" />
                                    </Button>
                                </SheetTrigger>
                                <SheetContent side="right" className="w-64 bg-neutral-900 text-white">
                                    <div className="space-y-1 px-2 pb-3 pt-2">
                                        <Link
                                            href="/"
                                            className="block rounded-md px-3 py-2 text-base font-medium text-gray-300 hover:bg-neutral-800 hover:text-blue-400"
                                        >
                                            Duyệt
                                        </Link>
                                        <Link
                                            href="/following"
                                            className="block rounded-md px-3 py-2 text-base font-medium text-gray-300 hover:bg-neutral-800 hover:text-blue-400"
                                        >
                                            Đang theo dõi
                                        </Link>
                                        <Link
                                            href="/schedule"
                                            className="block rounded-md px-3 py-2 text-base font-medium text-gray-300 hover:bg-neutral-800 hover:text-blue-400"
                                        >
                                            Lịch phát hành
                                        </Link>
                                    </div>
                                    <div className="border-t border-neutral-700 pb-3 pt-4">
                                        {session ? (
                                            <div className="space-y-1 px-2">
                                                <div className="flex flex-col space-y-1 py-2">
                                                    <p className="text-xs leading-none text-muted-foreground">
                                                        Xin chào
                                                    </p>
                                                    <p className="truncate text-sm font-medium leading-none">
                                                        {session.username}
                                                    </p>
                                                    <div className="py-2">
                                                        <KanaCoinDropDown session={session} />
                                                    </div>
                                                    <div className="py-2">
                                                        <NotificationsPopover session={session} />
                                                    </div>
                                                </div>
                                                <Button
                                                    variant="ghost"
                                                    className="w-full justify-start rounded-md px-3 py-2 text-left text-base font-medium text-gray-300 hover:bg-neutral-800 hover:text-blue-400"
                                                >
                                                    <User className="mr-2 h-4 w-4" />
                                                    Hồ sơ
                                                </Button>
                                                <Button
                                                    variant="ghost"
                                                    asChild
                                                    className="w-full justify-start rounded-md px-3 py-2 text-left text-base font-medium text-gray-300 hover:bg-neutral-800 hover:text-blue-400"
                                                >
                                                    <Link href="/account/subscriptions">
                                                        <Settings className="mr-2 h-4 w-4" />
                                                        Cài đặt
                                                    </Link>
                                                </Button>
                                                <Button
                                                    asChild
                                                    variant="ghost"
                                                    className="w-full justify-start rounded-md px-3 py-2 text-left text-base font-medium text-gray-300 hover:bg-neutral-800 hover:text-blue-400"
                                                >
                                                    <Link href="/author-studio/series">
                                                        <PenTool className="mr-2 h-4 w-4" />
                                                        Xưởng truyện
                                                    </Link>
                                                </Button>
                                                <Button
                                                    onClick={handleLogout}
                                                    variant="ghost"
                                                    className="w-full justify-start rounded-md px-3 py-2 text-left text-base font-medium text-gray-300 hover:bg-neutral-800 hover:text-blue-400"
                                                >
                                                    <LogOut className="mr-2 h-4 w-4" />
                                                    Đăng xuất
                                                </Button>
                                            </div>
                                        ) : (
                                            <div className="space-y-1 px-2">
                                                <Link href="/login">
                                                    <Button
                                                        variant="ghost"
                                                        className="w-full rounded-md px-3 py-2 text-left text-base font-medium text-gray-300 hover:bg-neutral-800 hover:text-blue-400"
                                                    >
                                                        Đăng nhập
                                                    </Button>
                                                </Link>
                                                <Link href="/register">
                                                    <Button className="w-full bg-blue-600 text-white hover:bg-blue-700">
                                                        Đăng ký
                                                    </Button>
                                                </Link>
                                            </div>
                                        )}
                                    </div>
                                </SheetContent>
                            </Sheet>
                        </div>
                    </div>
                </div>
            </div>
        </header>
    );
}
