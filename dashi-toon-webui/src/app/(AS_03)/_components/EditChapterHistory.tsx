/* eslint-disable @next/next/no-img-element */
"use client";
import React, { ReactNode, useEffect, useMemo, useState } from "react";

import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { FaSave, FaTrash, FaUser } from "react-icons/fa";

import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";

import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";

import { FaHistory } from "react-icons/fa";
import { BsDot } from "react-icons/bs";
import Image from "next/image";
import { Input } from "@/components/ui/input";
import { DatePickerWithRange } from "@/components/ui/date-range-picker";
import { Toggle } from "@/components/ui/toggle";
import { NumberInput } from "@/components/NumberInput";
import { addDays, formatDistanceToNow } from "date-fns";
import { DateRange } from "react-day-picker";
import {
    ChapterContent,
    ChapterVersion,
    ChapterVersionResponse,
    deleteChapterVersion,
    editChapterVersionSummary,
    getChapterVersions,
    restoreChapterVersion,
} from "@/utils/api/author-studio/chapter";
import { toast } from "sonner";
import { getSeriesInfo } from "@/utils/api/author-studio/series";
import { Textarea } from "@/components/ui/textarea";

export interface EditChapterHistoryProps {
    children?: ReactNode;
    seriesId: string;
    volumeId: string;
    chapterId: string;
}

export default function EditChapterHistory(props: EditChapterHistoryProps) {
    const { children, seriesId, volumeId, chapterId } = props;
    const [timeFilter, setTimeFilter] = React.useState<DateRange | undefined>();
    const [search, setSearch] = useState<string>("");
    const [page, setPage] = useState<number>(1);
    const [autoSave, setAutoSave] = useState<boolean>(true);
    const [versions, setVersions] = useState<ChapterVersionResponse>();
    const [isComic, setIsComic] = useState<boolean>();

    async function fetchData() {
        if (isComic === undefined) return;

        let from = timeFilter?.from;
        let to = timeFilter?.to;

        if (from !== undefined && to === undefined) {
            to = from;
            from = undefined;
        }

        const [data, err] = await getChapterVersions(
            isComic,
            seriesId,
            volumeId,
            chapterId,
            search,
            autoSave,
            page,
            50,
            from,
            to,
        );

        if (err) {
            return;
        }

        setVersions({
            ...data,
        });
    }

    useEffect(() => {
        async function work() {
            const [seriesData, seriesErr] = await getSeriesInfo(seriesId);
            if (seriesErr) {
                toast.error("Không tải được nội dung chương. Vui lòng tải lại trang.");

                return;
            }

            setIsComic(seriesData.type.toString() === "2");
        }

        work();
    }, [seriesId]);

    useEffect(() => {
        fetchData();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isComic]);

    async function handleQuery() {
        await fetchData();
    }

    return (
        <Sheet>
            <SheetTrigger onClick={() => fetchData()}>{children}</SheetTrigger>
            <SheetContent className="flex w-full max-w-[min(90vw,600px)] flex-col sm:max-w-[min(90vw,600px)]">
                <SheetHeader>
                    <SheetTitle className="flex items-center gap-2 text-xl">
                        <FaHistory /> Version History
                    </SheetTitle>
                    <SheetDescription className="text-left">
                        View, compare, and restore previous versions of this chapter
                    </SheetDescription>
                </SheetHeader>

                <div className="w-full overflow-y-auto">
                    <div className="flex flex-wrap justify-end gap-2">
                        <Toggle
                            variant={"outline"}
                            className="flex gap-2"
                            pressed={autoSave}
                            onPressedChange={setAutoSave}
                        >
                            <FaSave /> Include AutoSave
                        </Toggle>
                        <DatePickerWithRange onSelect={setTimeFilter} value={timeFilter} />
                        <Input
                            type="text"
                            placeholder="Search by version summary..."
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                        />
                        <div className="flex items-center gap-2">
                            <p>Page</p>
                            <NumberInput placeholder="Page" value={page} onChange={(e) => e > 0 && setPage(e)} />
                        </div>
                        <Button className="bg-blue-600 text-white hover:bg-blue-700" onClick={handleQuery}>
                            Go
                        </Button>
                    </div>
                    <Accordion type="single" collapsible>
                        {versions?.items?.map((x) => (
                            <AccordionItem key={x.versionId} value={x.versionId}>
                                <AccordionTrigger>
                                    <HistoryHeader data={x} />
                                </AccordionTrigger>
                                <AccordionContent>
                                    <HistoryContent
                                        data={x}
                                        seriesId={seriesId}
                                        volumeId={volumeId}
                                        chapterId={chapterId}
                                        onChange={(summary) => {
                                            setVersions({
                                                ...versions,
                                                items: versions.items.map((y) =>
                                                    y.versionId === x.versionId
                                                        ? {
                                                              ...y,
                                                              versionName: summary,
                                                          }
                                                        : y,
                                                ),
                                            });
                                        }}
                                        onDelete={() => {
                                            setVersions({
                                                ...versions,
                                                items: versions.items.filter((y) => y.versionId !== x.versionId),
                                            });
                                        }}
                                    />
                                </AccordionContent>
                            </AccordionItem>
                        ))}
                    </Accordion>
                </div>
            </SheetContent>
        </Sheet>
    );
}

const exampleData: ChapterVersion = {
    versionId: "1",
    timestamp: new Date().toString(),
    versionName: "Save Draft @ 12:22AM 23/12/2023",
    isAutoSave: true,
    isCurrent: true,
    isPublished: true,
    title: "Chapter 1: The Beginning",
    content: [
        {
            imageUrl: "/images/600x800.png",
            imageName: "image",
            imageSize: 1000,
            imageWidth: 600,
            imageHeight: 800,
        },
        {
            imageUrl: "/images/600x800.png",
            imageName: "image",
            imageSize: 1000,
            imageWidth: 600,
            imageHeight: 800,
        },
        {
            imageUrl: "/images/600x800.png",
            imageName: "image",
            imageSize: 1000,
            imageWidth: 600,
            imageHeight: 800,
        },
        {
            imageUrl: "/images/600x800.png",
            imageName: "image",
            imageSize: 1000,
            imageWidth: 600,
            imageHeight: 800,
        },
        {
            imageUrl: "/images/600x800.png",
            imageName: "image",
            imageSize: 1000,
            imageWidth: 600,
            imageHeight: 800,
        },
        {
            imageUrl: "/images/600x800.png",
            imageName: "image",
            imageSize: 1000,
            imageWidth: 600,
            imageHeight: 800,
        },
    ],
    note: "This is a note",
    thumbnail: "/images/600x800.png",
};

interface HistoryHeaderProps {
    data: ChapterVersion;
}

function HistoryHeader(props: HistoryHeaderProps) {
    const { data } = props;

    return (
        <div className="flex items-center gap-2">
            <Image
                src={data.thumbnail ?? "/images/600x800.png"}
                alt="thumbnail"
                width={3 * 12}
                height={4 * 12}
                objectFit={"contain"}
            />
            <div className="text-left">
                <FaUser className="me-1 inline text-sm text-muted-foreground" />
                <span className="text-sm text-muted-foreground">Author</span>
                <BsDot className="inline text-sm text-muted-foreground" />
                <span className="text-sm text-muted-foreground">{formatDistanceToNow(Date.parse(data.timestamp))}</span>

                <br />
                <span className="me-2">{data.versionName}</span>
                {data.isAutoSave && <Badge className="me-2">Auto Save</Badge>}
                {data.isCurrent && <Badge className="me-2 bg-blue-600 text-white">Current</Badge>}
                {data.isPublished && <Badge className="me-2 bg-green-600 text-white">Published</Badge>}
            </div>
        </div>
    );
}

interface HistoryContentProps {
    seriesId: string;
    volumeId: string;
    chapterId: string;
    data: ChapterVersion;
    onChange?: (summary: string) => void;
    onDelete?: () => void;
}

function HistoryContent(props: HistoryContentProps) {
    const { data, seriesId, volumeId, chapterId, onChange, onDelete } = props;
    const [versionName, setVersionName] = useState<string>(data?.versionName ?? "");

    const isNovel = typeof data.content === "string";
    async function handleRevert() {
        const [res, err] = await restoreChapterVersion(seriesId, volumeId, chapterId, data.versionId);

        if (err) {
            toast.error("Failed to restore chapter version.");
            return;
        }

        toast.success("Chapter version restored successfully.");
        setTimeout(() => {
            window.location.reload();
        }, 1000);
    }

    async function handleSave() {
        const [_, err] = await editChapterVersionSummary(seriesId, volumeId, chapterId, data.versionId, versionName);

        if (err) {
            toast.error("Failed to save version summary.");
            return;
        }

        toast.success("Version summary saved successfully.");
        onChange?.(versionName);
    }

    async function handleDelete() {
        const [_, err] = await deleteChapterVersion(seriesId, volumeId, chapterId, data.versionId);

        if (err) {
            toast.error("Failed to delete chapter version.");
            return;
        }

        toast.success("Chapter version deleted successfully.");
        onDelete?.();
    }

    return (
        <div className="mx-1">
            <h4 className="mb-2">Tóm tắt phiên bản</h4>
            <div className="flex gap-2">
                <Input
                    type="text"
                    placeholder="Nhập tóm tắt phiên bản ở đây..."
                    value={versionName}
                    onChange={(e) => setVersionName(e.target.value)}
                    required
                />
                <Button className="flex gap-2 bg-blue-600 text-white hover:bg-blue-700" onClick={handleSave}>
                    <FaSave /> Lưu
                </Button>
            </div>

            <h4 className="mb-2 mt-4">Tiêu đề</h4>
            <Input type="text" placeholder="Tiêu đề chương" value={data.title} disabled />

            <h4 className="mb-2 mt-4">Nội dung</h4>

            {isNovel ? (
                <div
                    className="prose prose-neutral prose-invert h-[35vh] overflow-y-auto rounded-md border px-3 py-2 lg:prose-xl"
                    dangerouslySetInnerHTML={{ __html: data.content as string }}
                ></div>
            ) : (
                <div className="flex h-48 gap-2 overflow-x-auto rounded-lg border border-neutral-600 bg-neutral-800 p-2">
                    {(data.content as ChapterContent[]).map((x) => (
                        <img
                            className="border-lg relative aspect-[3/4] rounded-lg border border-neutral-600 object-contain"
                            src={x.imageUrl}
                            key={x.imageName}
                            alt="Trang chương"
                        />
                    ))}
                </div>
            )}

            <h4 className="mb-2 mt-4">Ghi chú của tác giả</h4>
            <Textarea placeholder="Ghi chú của tác giả" value={data.note} disabled />

            <div className="mt-4 flex items-center justify-end gap-2">
                <Button className="flex gap-2" variant={"destructive"} onClick={handleDelete}>
                    <FaTrash /> Xóa
                </Button>
                <Button className="flex gap-2 bg-blue-600 text-white hover:bg-blue-700" onClick={handleRevert}>
                    <FaHistory />
                    Khôi phục
                </Button>
            </div>
        </div>
    );
}
