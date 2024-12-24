"use client";

import { useState, useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import Image from "next/image";
import Link from "next/link";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Search, Book, User } from "lucide-react";
import { Badge } from "./ui/badge";
import { useDebounceValue } from "usehooks-ts";
import { search, SearchResponse } from "@/utils/api/series";
import cx from "classnames";

export interface SearchBarProps {
    pill?: boolean;
}

export default function SearchBar(props: SearchBarProps) {
    const { pill } = props;
    const [searchTermIm, setSearchTermIm] = useState("");
    const [searchTerm, setSearchTerm] = useDebounceValue("", 100);
    const [searchResults, setSearchResults] = useState<SearchResponse>();
    const [isSearching, setIsSearching] = useState<boolean>(false);
    const [showDropdown, setShowDropdown] = useState<boolean>(false);
    const dropdownRef = useRef<HTMLDivElement>(null);
    const router = useRouter();

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setShowDropdown(false);
            }
        };

        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    useEffect(() => {
        async function work() {
            if (!searchTerm) {
                setSearchResults(undefined);
                setShowDropdown(false);
                return;
            }

            setIsSearching(true);
            const [rs, err] = await search({
                term: searchTerm,
                pageSize: 5,
            });

            if (err) {
                setSearchResults(undefined);
                setIsSearching(false);
                setShowDropdown(false);
            }
            setIsSearching(false);
            setSearchResults(rs);
            setShowDropdown((rs?.items?.length ?? 0) > 0);
        }

        work();
    }, [searchTerm]);

    const handleSearch = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        router.push(`/search?q=${encodeURIComponent(searchTerm)}`);
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") {
            e.preventDefault();
            router.push(`/search?q=${encodeURIComponent(searchTerm)}`);
        }
    };

    return (
        <div className="relative w-full max-w-xl" ref={dropdownRef}>
            <form onSubmit={handleSearch} className="flex">
                <Input
                    type="search"
                    placeholder="Tìm kiếm truyện..."
                    value={searchTermIm}
                    onChange={(e) => {
                        setSearchTermIm(e.target.value);
                        setSearchTerm(e.target.value);
                    }}
                    onKeyDown={handleKeyDown}
                    className={cx(
                        "w-full rounded-r-none border-neutral-700 bg-neutral-800 text-neutral-100 focus:border-blue-500",
                        {
                            "rounded-l-full": pill,
                        },
                    )}
                    aria-label="Tìm kiếm truyện"
                />
                <Button
                    type="submit"
                    className={cx("rounded-l-none bg-blue-600 text-white hover:bg-blue-700", {
                        "rounded-r-full": pill,
                    })}
                >
                    <Search className="h-4 w-4" />
                    <span className="sr-only">Tìm kiếm</span>
                </Button>
            </form>
            {showDropdown && (searchResults?.items.length ?? 0) > 0 && (
                <div
                    className="absolute right-0 z-10 mt-1 w-full rounded-md border border-neutral-700 bg-neutral-800 shadow-lg"
                    style={{ minWidth: "calc(min(100vw - 50px, 500px))" }}
                >
                    <ul className="py-1">
                        {searchResults?.items?.map((result) => (
                            <li key={result.id} className="border-b-2">
                                <Link
                                    href={`/series/${result.id}`}
                                    className="flex items-center px-4 py-2 hover:bg-neutral-700"
                                >
                                    <div className="mr-4 w-14 flex-shrink-0">
                                        <Image
                                            src={result.thumbnail ?? "/images/placeholder.png"}
                                            alt={""}
                                            width={50}
                                            height={75}
                                            className="self-start object-cover"
                                        />
                                    </div>
                                    <div className="flex-1">
                                        <h3 className="text-left text-lg font-semibold text-neutral-100">
                                            {result.title}
                                        </h3>

                                        <p className="flex items-center gap-1 text-sm text-neutral-400">
                                            <User size={16} />
                                            {result.authors.length === 0
                                                ? "Không có thông tin tác giả"
                                                : result.authors.join(", ")}
                                        </p>
                                        <div className="mt-2 flex flex-wrap gap-2">
                                            {result.genres.map((x) => (
                                                <Badge key={x}>{x}</Badge>
                                            ))}
                                        </div>
                                    </div>
                                </Link>
                            </li>
                        ))}
                    </ul>
                </div>
            )}
            {isSearching && (
                <div className="absolute z-10 mt-1 w-full rounded-md border border-neutral-700 bg-neutral-800 p-4 text-center text-neutral-100 shadow-lg">
                    Đang tìm kiếm...
                </div>
            )}
        </div>
    );
}
