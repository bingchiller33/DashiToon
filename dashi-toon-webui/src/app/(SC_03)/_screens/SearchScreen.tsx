"use client";

import React, { useState, useEffect } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useInView } from "react-intersection-observer";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Search, Book, BookOpen, Loader2, Filter } from "lucide-react";
import { search } from "@/utils/api/series";
import { FancyMultiSelect } from "@/components/FancyMultiSelect";
import * as env from "@/utils/env";
import { RATING_CONFIG_2, STATUS_CONFIG_2 } from "@/utils/consts";
import SiteLayout from "@/components/SiteLayout";
import SeriesCard from "@/components/SeriesCard";
import { Checkbox } from "@/components/ui/checkbox";
import { se } from "date-fns/locale";

type Genre = {
    id: number;
    name: string;
};

type SelectableItem = {
    value: number;
    label: string;
};

export default function SearchPage() {
    const searchParams = useSearchParams();
    const [searchTerm, setSearchTerm] = useState(searchParams.get("q") || "");
    const [selectedTypes, setSelectedTypes] = useState<number[]>([]);
    const [selectedStatus, setSelectedStatus] = useState<string[]>([]);
    const [selectedContentRating, setSelectedContentRating] = useState<string[]>([]);
    const [genres, setGenres] = useState<Genre[]>([]);
    const [selectedGenres, setSelectedGenres] = useState<SelectableItem[]>([]);

    useEffect(() => {
        const fetchGenres = async () => {
            try {
                const response = await fetch(`${env.getBackendHost()}/api/Genres`);
                const data: Genre[] = await response.json();
                setGenres(data);
            } catch (error) {
                console.error("Error fetching genres", error);
            }
        };

        fetchGenres();
    }, []);

    const { ref, inView } = useInView();

    const { data, fetchNextPage, hasNextPage, isFetchingNextPage, status } = useInfiniteQuery({
        initialPageParam: 0,
        queryKey: ["novels", searchTerm, selectedGenres, selectedTypes, selectedStatus, selectedContentRating],
        queryFn: async ({ pageParam = 0 }) => {
            const [data, err] = await search({
                term: searchTerm,
                type: selectedTypes,
                status: selectedStatus,
                contentRating: selectedContentRating,
                genres: selectedGenres.length > 0 ? selectedGenres.map((x) => x.label) : undefined,
                pageNumber: pageParam + 1,
                pageSize: 12,
            });

            if (err) {
                throw err;
            }

            return data;
        },
        getNextPageParam: (lastPage, allPages) => {
            const nextPage = allPages.length;
            return nextPage * 10 < lastPage.totalCount ? nextPage : undefined;
        },
    });

    useEffect(() => {
        if (inView && hasNextPage) {
            fetchNextPage();
        }
    }, [inView, fetchNextPage, hasNextPage]);

    function modifyArr<T>(input: T[], item: T, isAdded: boolean) {
        return isAdded ? [...input, item] : input.filter((x) => x !== item);
    }

    return (
        <SiteLayout>
            <main className="container mx-auto px-4 py-8">
                <h1 className="text-3xl font-bold">Tìm kiếm truyện</h1>
                <p className="mb-6 text-muted-foreground">
                    Tìm kiếm tất cả truyện tranh và tiểu thuyết trên nên tảng đọc truyện trực tuyến DashiToon!
                </p>
                <div className="flex flex-col gap-8 lg:flex-row">
                    <aside className="w-full lg:w-64">
                        <Accordion type="single" collapsible className="w-full" defaultValue="advanced-filtering">
                            <AccordionItem value="advanced-filtering">
                                <AccordionTrigger>
                                    <p className="flex gap-2">
                                        <Filter /> Bộ lọc nâng cao
                                    </p>
                                </AccordionTrigger>
                                <AccordionContent>
                                    <div className="space-y-6">
                                        <div>
                                            <h3 className="mb-2 font-semibold">Thể loại</h3>
                                            <div className="space-y-2">
                                                <FancyMultiSelect
                                                    items={genres.map((genre) => ({
                                                        value: genre.id,
                                                        label: genre.name,
                                                    }))}
                                                    selectedItems={selectedGenres}
                                                    onSelectionChange={(newSelectedItems) => {
                                                        setSelectedGenres(newSelectedItems);
                                                    }}
                                                />
                                            </div>
                                        </div>
                                        <div>
                                            <h3 className="mb-2 font-semibold">Loại truyện</h3>
                                            <div className="my-1 flex items-center space-x-2">
                                                <Checkbox
                                                    id="type-1"
                                                    checked={selectedTypes.includes(1)}
                                                    onCheckedChange={(x) =>
                                                        setSelectedTypes(modifyArr(selectedTypes, 1, !!x))
                                                    }
                                                />
                                                <label htmlFor="type-1">Tiểu thuyết</label>
                                            </div>
                                            <div className="flex items-center space-x-2">
                                                <Checkbox
                                                    id="type-2"
                                                    checked={selectedTypes.includes(2)}
                                                    onCheckedChange={(x) =>
                                                        setSelectedTypes(modifyArr(selectedTypes, 2, !!x))
                                                    }
                                                />
                                                <label htmlFor="type-2">Truyện tranh</label>
                                            </div>
                                        </div>
                                        <div>
                                            <h3 className="mb-2 font-semibold">Trạng thái</h3>
                                            {Object.entries(STATUS_CONFIG_2)
                                                .filter((x) => x[0] !== "4")
                                                .map(([id, { label }]) => (
                                                    <div
                                                        className="my-1 flex items-center space-x-2"
                                                        key={`status-${id}`}
                                                    >
                                                        <Checkbox
                                                            id={`status-${id}`}
                                                            checked={selectedStatus.includes(id)}
                                                            onCheckedChange={(x) =>
                                                                setSelectedStatus(modifyArr(selectedStatus, id, !!x))
                                                            }
                                                        />
                                                        <label htmlFor={`status-${id}`}>{label}</label>
                                                    </div>
                                                ))}
                                        </div>
                                        <div>
                                            <h3 className="mb-2 font-semibold">Xếp hạng nội dung</h3>
                                            {Object.entries(RATING_CONFIG_2).map(([id, { label }]) => (
                                                <div key={id} className="my-1 flex items-center space-x-2">
                                                    <Checkbox
                                                        id={`rating-${id}`}
                                                        checked={selectedContentRating.includes(id)}
                                                        onCheckedChange={(x) =>
                                                            setSelectedContentRating(
                                                                modifyArr(selectedContentRating, id, !!x),
                                                            )
                                                        }
                                                    />
                                                    <label htmlFor={`rating-${id}`}>{label}</label>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                </AccordionContent>
                            </AccordionItem>
                        </Accordion>
                    </aside>
                    {/* Main content area */}
                    <div className="flex-1">
                        <form className="mb-6">
                            <div className="flex gap-2">
                                <Input
                                    type="search"
                                    placeholder="Nhập tên truyện, tác giả, hoặc thể loại..."
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                    className="flex-1 border-neutral-700 bg-neutral-800 focus:border-blue-500"
                                />
                                <Button className="bg-blue-600 text-white hover:bg-blue-700" asChild>
                                    <Link href={`/search?q=${searchTerm}`}>
                                        <Search className="mr-2 h-4 w-4" />
                                        Tìm kiếm
                                    </Link>
                                </Button>
                            </div>
                        </form>
                        {(status as any) === "loading" ? (
                            <div className="flex h-64 items-center justify-center">
                                <Loader2 className="h-8 w-8 animate-spin" />
                            </div>
                        ) : status === "error" ? (
                            <div className="text-center text-red-500">Đã xảy ra lỗi khi tải dữ liệu.</div>
                        ) : (
                            <>
                                <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                                    {data?.pages.map((page, i) => (
                                        <React.Fragment key={i}>
                                            {page?.items?.map((series) => <SeriesCard key={series.id} data={series} />)}
                                        </React.Fragment>
                                    ))}
                                </div>
                                <div ref={ref} className="mt-4 flex h-20 items-center justify-center">
                                    {isFetchingNextPage ? (
                                        <Loader2 className="h-6 w-6 animate-spin" />
                                    ) : hasNextPage ? (
                                        <Button
                                            onClick={() => fetchNextPage()}
                                            className="bg-blue-600 hover:bg-blue-700"
                                        >
                                            Tải thêm
                                        </Button>
                                    ) : (
                                        <p>Không còn kết quả nào khác.</p>
                                    )}
                                </div>
                            </>
                        )}
                    </div>
                </div>
            </main>
        </SiteLayout>
    );
}
