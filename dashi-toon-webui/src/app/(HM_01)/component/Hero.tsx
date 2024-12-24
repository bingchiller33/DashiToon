import SearchBar from "@/components/SearchBar";
import { Button } from "@/components/ui/button";
import { ChevronDown, Search, Sparkles } from "lucide-react";
import Link from "next/link";
import React from "react";

export default function Hero({ onClick }: { onClick: () => void }) {
    return (
        <div className="hero-images relative">
            <div className="flex items-center bg-black/60 py-[134px]">
                <div className="container mx-auto px-4 text-center">
                    <h1 className="mb-6 text-4xl font-bold text-blue-800 dark:text-blue-200">
                        Khám Phá Câu Chuyện Yêu Thích Tiếp Theo Của Bạn
                    </h1>
                    <p className="mx-auto mb-8 max-w-2xl text-xl text-neutral-700 dark:text-neutral-300">
                        Đắm chìm trong những câu chuyện hấp dẫn, kết nối với những người yêu sách, và bắt đầu cuộc phiêu
                        lưu văn học của bạn.
                    </p>
                    <div className="flex justify-center">
                        <SearchBar pill />
                    </div>
                    <div className="mt-2 flex flex-wrap justify-center gap-x-4 gap-y-2">
                        <Button
                            size="lg"
                            className="flex items-center gap-2 rounded-full bg-blue-600 text-white hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600"
                            onClick={onClick}
                            asChild
                        >
                            <Link href="/search">
                                <Search />
                                Bắt Đầu Khám Phá
                            </Link>
                        </Button>

                        <Button
                            size="lg"
                            className="flex items-center gap-2 rounded-full bg-blue-600 text-white hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600"
                            onClick={onClick}
                        >
                            <Sparkles />
                            Truyện Được Đề Xuất
                        </Button>
                    </div>

                    <ChevronDown className="absolute bottom-0 left-1/2 -translate-x-1/2 transform animate-bounce text-blue-800 dark:text-blue-200" />
                </div>
            </div>
        </div>
    );
}
