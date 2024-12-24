/* eslint-disable @next/next/no-img-element */
"use client";

import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { Slider } from "@/components/ui/slider";
import { BookText, ChevronLeft, ChevronRight, CircleArrowLeft, CircleArrowRight, Menu, Settings } from "lucide-react";
import React, { useCallback, useEffect, useRef, useState } from "react";
import { useLocalStorage } from "usehooks-ts";

import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";

import ChapterMenuList from "@/app/(Reader)/components/ChapterMenuList";
import { UnlockChapter } from "@/app/(Reader)/components/UnlockChapter";
import { NumberInput } from "@/components/NumberInput";
import { Checkbox } from "@/components/ui/checkbox";
import { Separator } from "@/components/ui/separator";
import { toggleFullScreen } from "@/utils";
import { getChapterVersionPreview } from "@/utils/api/author-studio/chapter";
import {
    ChapterContent,
    ComicImage,
    getComicChapterContent,
    getSeriesInfo,
    getUserAccessibleChapters,
    SeriesResp,
    VolumeChapters,
} from "@/utils/api/series";
import cx from "classnames";
import { BookOpen, ChevronsDownUp, ChevronsRightLeft } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { toast } from "sonner";
import { useChapVis } from "@/hooks/useChapVis";
import * as env from "@/utils/env";
import CommentDialog from "../components/CommentDialog";
import { getSeriesInfo as getSeriesInfoAS } from "@/utils/api/author-studio/series";
import { ReportDialog } from "@/components/ReportDialog";
import { reportChapter } from "@/utils/api/reader/report";
import { useRouter } from "next/navigation";
import useUwU from "@/hooks/useUwU";
import { Label } from "@/components/ui/label";

type ThemeOption = {
    name: string;
    bgColor: string;
    textColor: string;
};

const themeOptions: ThemeOption[] = [
    { name: "Black", bgColor: "bg-black", textColor: "text-gray-100" },
    { name: "Dark Gray", bgColor: "bg-gray-800", textColor: "text-gray-100" },
    { name: "White", bgColor: "bg-white", textColor: "text-gray-900" },
    { name: "Cream", bgColor: "bg-[#F5F5DC]", textColor: "text-gray-900" },
    { name: "Light Blue", bgColor: "bg-[#E6F3FF]", textColor: "text-gray-900" },
    { name: "Mint", bgColor: "bg-[#E6FFF2]", textColor: "text-gray-900" },
];

const layoutOptions = [
    {
        name: "Single",
        icon: BookText,
        value: "single",
        layoutClass: "flex justify-center ",
    },
    {
        name: "Double",
        icon: BookOpen,
        value: "double",
        layoutClass: "grid grid-cols-2 mx-auto w-max justify-items-center",
    },
    {
        name: "Row",
        icon: ChevronsDownUp,
        value: "row",
        layoutClass: "flex flex-col",
    },
    {
        name: "Column",
        icon: ChevronsRightLeft,
        value: "column",
        layoutClass: "flex",
    },
];

const STORAGE_KEY = "readerComicSettings";

const defaultSettings = {
    currentTheme: themeOptions[0],
    layoutMode: layoutOptions[2].value,
    stripMargin: 12,
    dir: "ltr" as "ltr" | "rtl",
    tapToTurn: "directional" as "directional" | "alwaysForward" | "never",
    doubleTapFs: true,
    containWidth: true,
    isLimitWidth: false,
    limitWidth: 720,

    containHeight: true,
    isLimitHeight: false,
    limitHeight: 1280,

    stretchSmall: false,

    autoUnlock: false,
};

const dblClickInterval = 250;

export interface ReadComicScreenProps {
    chapterId: string;
    volumeId: string;
    seriesId: string;
    previewVersion?: string;
}

async function fetchNextChapterUrl(seriesId: string, volumeId: string, chapterId: string, vc: VolumeChapters) {
    console.log("fetchNextChapterUrl");
    const vol = vc.data.find((x) => x.volumeId.toString() === volumeId);
    if (!vol) {
        return null;
    }

    const current = vol.chapters.find((x) => x.id.toString() === chapterId);
    if (!current) {
        return null;
    }

    const next = vol.chapters.find((c) => c.chapterNumber === current.chapterNumber + 1);

    if (!next) {
        const curVol = vc.data.find((v) => v.volumeId.toString() === volumeId);
        if (!curVol) {
            return null;
        }

        const nextVol = vc.data.find((v) => v.volumeNumber > curVol.volumeNumber);

        if (!nextVol) {
            return null;
        }

        const chapters = [...nextVol.chapters];
        const firstChapter = chapters.sort((a, b) => a.chapterNumber - b.chapterNumber)[0];

        if (!firstChapter) {
            return null;
        }

        return `/series/${seriesId}/vol/${nextVol.volumeId}/chap/${firstChapter.id}/comic`;
    }

    return `/series/${seriesId}/vol/${volumeId}/chap/${next.id}/comic`;
}

async function fetchPrevChapterUrl(seriesId: string, volumeId: string, chapterId: string, vc: VolumeChapters) {
    const vol = vc.data.find((x) => x.volumeId.toString() === volumeId);
    if (!vol) {
        return null;
    }

    const current = vol.chapters.find((x) => x.id.toString() === chapterId);
    if (!current) {
        return null;
    }

    const prev = vol.chapters.find((c) => c.chapterNumber === current.chapterNumber - 1);

    if (!prev) {
        const curVol = vc.data.find((v) => v.volumeId.toString() === volumeId);
        if (!curVol) {
            return null;
        }

        const prevVol = vc.data.find((v) => v.volumeNumber === curVol.volumeNumber - 1);

        if (!prevVol) {
            return null;
        }

        const chapters = [...prevVol.chapters];
        const lastChapter = chapters.sort((a, b) => b.chapterNumber - a.chapterNumber)[0];
        if (!lastChapter) {
            return null;
        }

        return `/series/${seriesId}/vol/${prevVol.volumeId}/chap/${lastChapter.id}/comic`;
    }

    return `/series/${seriesId}/vol/${volumeId}/chap/${prev.id}/comic`;
}

const ReadComicScreen: React.FC<ReadComicScreenProps> = (props: ReadComicScreenProps) => {
    const router = useRouter();
    const [uwu] = useUwU();
    const { chapterId, volumeId, seriesId, previewVersion } = props;
    useChapVis(props.seriesId, props.chapterId, !!previewVersion);
    const [showUnlock, setShowUnlock] = useState(false);
    const [isAuthenticated, setIsAuthenticated] = useState(false);

    const contentAreaRef = useRef<HTMLElement>(null);
    const imgsRef = useRef<(HTMLImageElement | null)[]>([]);
    const progByTap = useRef(0);
    const lastClick = useRef(0);
    const hasTrackedView = useRef(false);
    const [autoUnlock, setAutoUnlock] = useState(false);

    const [showNav, setShowNav] = useState(true);
    const [progress, setProgress] = useState(0);
    const [seriesData, setSeriesData] = useState<SeriesResp | null>(null);
    const [chapterData, setChapterData] = useState<ChapterContent<ComicImage[]> | null>(null);
    const [comicData, setComicData] = useState<ComicImage[]>([]);
    const [activeImages, setActiveImage0] = useState<ComicImage[]>([]);
    const [nextChapterUrl, setNextChapterUrl] = useState<string | null>(null);
    const [prevChapterUrl, setPrevChapterUrl] = useState<string | null>(null);

    const trackChapterView = useCallback(async () => {
        if (hasTrackedView.current) return;
        try {
            const response = await fetch(`${env.getBackendHost()}/api/Chapters/${chapterId}/analytics/view`, {
                method: "POST",
            });

            if (response.ok) {
                console.log("Chapter view tracked");
                hasTrackedView.current = true;
            } else {
                console.error("Failed to track chapter view:", response.status);
            }
        } catch (error) {
            console.error("Failed to track chapter view:", error);
        }
    }, [chapterId]);

    useEffect(() => {
        if (!comicData.length) return;

        let timeSpent = 0;
        let lastTime = Date.now();
        let timerId: NodeJS.Timeout | null = null;

        const handleVisibilityChange = () => {
            if (document.hidden) {
                // Clear interval when page is hidden
                if (timerId) {
                    clearInterval(timerId);
                    timerId = null;
                }
            } else {
                // Reset last time and restart interval when page becomes visible
                lastTime = Date.now();
                if (!timerId) {
                    timerId = setInterval(tick, 1000);
                }
            }
        };

        const tick = () => {
            const now = Date.now();
            const delta = now - lastTime;
            lastTime = now;

            timeSpent += delta;

            if (timeSpent >= 15000) {
                trackChapterView();
                if (timerId) {
                    clearInterval(timerId);
                }
                document.removeEventListener("visibilitychange", handleVisibilityChange);
            }
        };

        // Start tracking time
        timerId = setInterval(tick, 1000);

        // Add visibility change listener
        document.addEventListener("visibilitychange", handleVisibilityChange);

        return () => {
            if (timerId) {
                clearInterval(timerId);
            }
            document.removeEventListener("visibilitychange", handleVisibilityChange);
        };
    }, [comicData, trackChapterView]);

    function setActiveImage(arg: React.SetStateAction<ComicImage[]>) {
        if (typeof arg === "function") {
            setActiveImage0((prev) => arg(prev.filter((x) => x !== undefined)));
        } else {
            setActiveImage0(arg.filter((x) => x !== undefined));
        }
    }
    const [settings, setSettings, clearSettings] = useLocalStorage(STORAGE_KEY, defaultSettings, {
        initializeWithValue: false,
    });

    const {
        currentTheme,
        layoutMode,
        stripMargin,
        dir,
        tapToTurn,
        doubleTapFs,
        containHeight,
        containWidth,
        isLimitHeight,
        isLimitWidth,
        limitHeight,
        limitWidth,
        stretchSmall,
    } = settings;

    useEffect(() => {
        async function workNormal() {
            const [data, err] = await getComicChapterContent(seriesId, volumeId, chapterId);
            if (err) {
                if (err.status === 401) {
                    setShowUnlock(true);
                    setIsAuthenticated(false);
                } else if (err.status === 403) {
                    setShowUnlock(true);
                    setIsAuthenticated(true);
                } else {
                    toast.error("Không tải được dữ liệu truyện tranh, vui lòng thử lại sau!");
                    return;
                }
            }
            setChapterData(data!);
            setComicData(data!.content);

            const [seriesData, err2] = await getSeriesInfo(seriesId);
            if (err2) {
                toast.error("Không tải được dữ liệu truyện tranh, vui lòng thử lại sau!");
                return;
            }

            setSeriesData(seriesData);

            const [vc, errVc] = await getUserAccessibleChapters(seriesId);
            if (errVc) {
                toast.error("Không tải được dữ liệu truyện tranh, vui lòng thử lại sau!");
                return;
            }

            const next = await fetchNextChapterUrl(seriesId, volumeId, chapterId, vc);

            setNextChapterUrl(next);

            const prev = await fetchPrevChapterUrl(seriesId, volumeId, chapterId, vc);

            setPrevChapterUrl(prev);
        }

        async function workPreview() {
            const [data, err] = await getChapterVersionPreview(seriesId, volumeId, chapterId, previewVersion!);

            if (err) {
                toast.error("Không tải được dữ liệu truyện tranh, vui lòng thử lại sau!");
                return;
            }
            setChapterData({
                id: data.chapterId,
                ...data,
            });
            setComicData(data.content);

            const [seriesData, err2] = await getSeriesInfoAS(seriesId);
            if (err2) {
                toast.error("Không tải được dữ liệu truyện tranh, vui lòng thử lại sau!");
                return;
            }

            setSeriesData(seriesData as any);
        }

        if (previewVersion) {
            workPreview();
        } else {
            workNormal();
        }
    }, [seriesId, volumeId, chapterId, previewVersion]);

    useEffect(() => {
        const currentRef = contentAreaRef.current;
        if (!currentRef) return;

        function handleScroll() {
            if (progByTap.current + 1000 > +new Date()) return;
            const scroll = 250 + (layoutMode === "row" ? currentRef!.scrollTop : currentRef!.scrollLeft);

            const activeIndex = imgsRef.current.findIndex((img) => {
                if (!img) return false;
                const imgLow = layoutMode === "row" ? img.offsetTop : img.offsetLeft;
                const imgHigh = imgLow + (layoutMode === "row" ? img.clientHeight : img.clientWidth);
                return imgLow <= scroll && scroll < imgHigh;
            });

            if (activeIndex !== -1) {
                setProgress(activeIndex);
                console.log("progress 1", activeIndex);
            }
        }

        currentRef.addEventListener("scroll", handleScroll);

        return () => {
            currentRef.removeEventListener("scroll", handleScroll);
        };
    }, [layoutMode, progByTap]);

    useEffect(() => {
        const currentRef = contentAreaRef.current;
        if (!currentRef) return;

        function next() {
            if (progress === comicData.length - 1) {
                if (nextChapterUrl) {
                    router.push(nextChapterUrl);
                }
                return;
            }

            setProgress((p) => (p < comicData.length - 1 ? p + 1 : p));
            progByTap.current = +new Date();
        }

        function prev() {
            if (progress === 0) {
                if (prevChapterUrl) {
                    router.push(prevChapterUrl);
                }
                return;
            }
            setProgress((p) => (p > 0 ? p - 1 : p));
            progByTap.current = +new Date();
        }

        function handleClick(e: MouseEvent) {
            const isDblClick = +new Date() - lastClick.current < dblClickInterval;
            lastClick.current = +new Date();

            if (tapToTurn === "never") return;

            if (e.clientX < window.innerWidth / 3) {
                console.log("left");
                if (tapToTurn === "directional") prev();
                else next();
            } else if (e.clientX > (window.innerWidth / 3) * 2) {
                console.log("right");
                next();
            } else {
                console.log("middle");
                if (isDblClick) {
                    toggleFullScreen();
                    console.log("double");
                } else {
                    setShowNav((o) => !o);
                }
            }
        }

        function handleKey(e: KeyboardEvent) {
            const leftKeys = ["ArrowLeft", "a", "A", "PageUp"];
            const rightKeys = ["ArrowRight", "d", "D", "PageDown"];
            if (rightKeys.includes(e.key)) {
                next();
            } else if (leftKeys.includes(e.key)) {
                prev();
            }
        }

        currentRef.addEventListener("click", handleClick);
        document.addEventListener("keyup", handleKey);
        return () => {
            currentRef.removeEventListener("click", handleClick);
            document.removeEventListener("keyup", handleKey);
        };
    }, [tapToTurn, comicData, progress, nextChapterUrl, prevChapterUrl, router]);

    useEffect(() => {
        switch (layoutMode) {
            case "single":
                setActiveImage([comicData[progress]]);
                break;
            case "double":
                if (progress === comicData.length - 1) {
                    setActiveImage([comicData[progress - 1], comicData[progress]]);
                } else {
                    setActiveImage([comicData[progress], comicData[progress + 1]]);
                }
                break;
            case "row":
            case "column":
                setActiveImage(comicData);
                imgsRef.current[progress]?.scrollIntoView({
                    behavior: "smooth",
                });
                break;
        }
        console.log("prgre", progress);
    }, [layoutMode, progress, comicData]);

    const resetSettings = () => {
        clearSettings();
    };

    const fetchContent = async () => {
        if (previewVersion) {
            const [data, err] = await getChapterVersionPreview(seriesId, volumeId, chapterId, previewVersion!);

            if (err) {
                toast.error("Không tải được dữ liệu truyện tranh, vui lòng thử lại sau!");
                return;
            }
            setChapterData({
                id: data.chapterId,
                ...data,
            });
            setComicData(data.content);
        } else {
            const [data, err] = await getComicChapterContent(seriesId, volumeId, chapterId);
            if (err) {
                if (err.status === 401) {
                    setShowUnlock(true);
                    setIsAuthenticated(false);
                } else if (err.status === 403) {
                    setShowUnlock(true);
                    setIsAuthenticated(true);
                } else {
                    console.log("Không tải được dữ liệu truyện tranh, vui lòng thử lại sau!");
                }
                return;
            }
            setChapterData(data!);
            setComicData(data!.content);
        }
    };

    const handleReport = async (
        contentId: string,
        reason: string,
        contentType: "series" | "chapter" | "review" | "comment",
    ) => {
        try {
            const [error] = await reportChapter(parseInt(contentId), reason);
            if (error) {
                toast.error("Không thể gửi báo cáo. Vui lòng thử lại sau");
                return;
            }
            toast.success("Đã gửi báo cáo thành công");
        } catch (error) {
            console.error("Error:", error);
            toast.error("Đã xảy ra lỗi khi gửi báo cáo");
        }
    };

    return (
        <div
            className={cx(
                "relative flex h-screen w-full overflow-auto transition-colors duration-300",
                currentTheme.textColor,
                {
                    [currentTheme.bgColor]: true,
                },
            )}
        >
            <main
                ref={contentAreaRef}
                className={cx(
                    "align-safe-center relative w-full select-none self-center overflow-auto transition-colors duration-300",
                    layoutOptions.find((l) => l.value === layoutMode)?.layoutClass,
                    {
                        "h-[calc(100vh-200px)]": !uwu,
                        "h-screen": uwu,
                    },
                )}
                style={{
                    gap: `${stripMargin}px`,
                    direction: settings.dir ?? "ltr",
                }}
            >
                {showUnlock ? (
                    <UnlockChapter
                        isAuthenticated={isAuthenticated}
                        timeUntilFree="22h : 35m : 01"
                        seriesId={seriesId}
                        volumeId={volumeId}
                        chapterId={chapterId}
                        onUnlockSuccess={() => {
                            setShowUnlock(false);
                            fetchContent();
                        }}
                        autoUnlock={autoUnlock}
                        onAutoUnlockChange={(checked) => setAutoUnlock(checked)}
                    />
                ) : (
                    activeImages.map((image, index) => (
                        <Image
                            width={image.imageWidth}
                            height={image.imageHeight}
                            ref={(el) => {
                                imgsRef.current[index] = el;
                            }}
                            src={image.imageUrl}
                            alt="comic"
                            key={index}
                            style={{
                                width: isLimitWidth ? `${limitWidth}px` : "auto",
                                height: isLimitHeight ? `${limitHeight}px` : "auto",
                            }}
                            className={cx("block object-contain", {
                                "max-w-full": containWidth && !isLimitWidth,
                                "max-h-full": containHeight && !isLimitHeight,
                                "min-h-full": stretchSmall && !isLimitHeight,
                                "max-w-none": isLimitWidth,
                                "max-h-none": isLimitHeight,
                            })}
                        />
                    ))
                )}
            </main>

            <header
                style={{ direction: "initial" }}
                className={`fixed left-0 right-0 top-0 z-10 border-b ${currentTheme.bgColor} transition-transform duration-300 ${showNav ? "translate-y-0" : "-translate-y-full"} w-screen py-4`}
            >
                <div className="mx-auto flex max-w-6xl items-center justify-between px-4">
                    <Link className="flex items-center space-x-4" href={`/series/${seriesId}`}>
                        <div className="h-12 w-12 rounded-lg bg-gray-200">
                            <Image
                                src={seriesData?.thumbnail ?? "/images/720x1280.png"}
                                alt="thumbnail"
                                width={48}
                                height={48}
                                className="h-12 rounded-lg object-cover"
                            />
                        </div>
                        <h1 className="text-xl font-bold">{seriesData?.title ?? "Đang tải..."}</h1>
                    </Link>
                    <div className="flex space-x-2">
                        <Link href={prevChapterUrl ?? "#"}>
                            <Button
                                disabled={!prevChapterUrl}
                                variant="ghost"
                                size="icon"
                                className={`${currentTheme.textColor}`}
                            >
                                <ChevronLeft />
                            </Button>
                        </Link>
                        <Link href={nextChapterUrl ?? "#"}>
                            <Button
                                disabled={!nextChapterUrl}
                                variant="ghost"
                                size="icon"
                                className={`${currentTheme.textColor}`}
                            >
                                <ChevronRight />
                            </Button>
                        </Link>
                    </div>
                </div>
            </header>

            <nav
                style={{ direction: "initial" }}
                className={`fixed bottom-0 left-0 right-0 border-t ${currentTheme.bgColor} transition-transform duration-300 ${showNav ? "translate-y-0" : "translate-y-full"}`}
            >
                <div className="mx-auto max-w-6xl px-4">
                    <div className="flex h-16 items-center justify-between">
                        <Sheet>
                            <SheetTrigger asChild>
                                <Button
                                    disabled={previewVersion !== undefined}
                                    title={previewVersion ? "Preview Mode" : ""}
                                    variant="ghost"
                                    size="icon"
                                    className={`${currentTheme.textColor}`}
                                >
                                    <Menu />
                                </Button>
                            </SheetTrigger>

                            <ChapterMenuList
                                seriesId={seriesId}
                                volumeId={volumeId}
                                chapterId={chapterId}
                                type="comic"
                            />
                        </Sheet>

                        <div className="flex flex-1 items-center px-4">
                            <h2 className="mr-2 text-sm font-bold">
                                Chương {chapterData?.chapterNumber ?? 1}: {chapterData?.title ?? "Loading"}
                            </h2>
                            <ReportDialog
                                contentId={seriesId}
                                contentType="series"
                                contentTitle={seriesData?.title}
                                onReport={handleReport}
                                iconSize={6}
                            />
                        </div>

                        <CommentDialog buttonVariant="icon" chapterId={chapterId} currentTheme={currentTheme} />

                        <Sheet>
                            <SheetTrigger asChild>
                                <Button variant="ghost" size="icon" className={`${currentTheme.textColor}`}>
                                    <Settings />
                                </Button>
                            </SheetTrigger>
                            <SheetContent side="right" className="flex w-full max-w-[90dvw] flex-col">
                                <SheetHeader>
                                    <SheetTitle className="text-2xl">Cài đặt trang</SheetTitle>
                                </SheetHeader>
                                <ScrollArea className="flex-grow pr-4">
                                    <Accordion defaultValue="item-1" type="single" collapsible>
                                        <AccordionItem value="item-1">
                                            <AccordionTrigger className="text-xl">Bố cục trang</AccordionTrigger>
                                            <AccordionContent>
                                                <h3>Kiểu hiển thị trang</h3>
                                                <div className="mb-4 mt-2 flex justify-between gap-1">
                                                    {layoutOptions.map((layout) => (
                                                        <Button
                                                            key={layout.value}
                                                            variant={
                                                                layoutMode === layout.value ? "default" : "outline"
                                                            }
                                                            title={layout.name}
                                                            className="w-full"
                                                            onClick={() =>
                                                                setSettings({
                                                                    ...settings,
                                                                    layoutMode: layout.value,
                                                                })
                                                            }
                                                        >
                                                            <layout.icon />
                                                        </Button>
                                                    ))}
                                                </div>

                                                <h3>Lề</h3>
                                                <div className="mb-4 mt-2 flex gap-2">
                                                    <NumberInput
                                                        value={settings.stripMargin}
                                                        onChange={(e) =>
                                                            setSettings({
                                                                ...settings,
                                                                stripMargin: e,
                                                            })
                                                        }
                                                    />
                                                    <Button
                                                        variant={"outline"}
                                                        onClick={() =>
                                                            setSettings({
                                                                ...settings,
                                                                stripMargin: defaultSettings.stripMargin,
                                                            })
                                                        }
                                                    >
                                                        Đặt lại lề
                                                    </Button>
                                                </div>

                                                <h3>Hướng đọc</h3>
                                                <div className="mb-4 mt-2 flex justify-between gap-1">
                                                    <Button
                                                        variant={dir === "ltr" ? "default" : "outline"}
                                                        className="w-full"
                                                        onClick={() =>
                                                            setSettings({
                                                                ...settings,
                                                                dir: "ltr",
                                                            })
                                                        }
                                                    >
                                                        <CircleArrowRight />
                                                    </Button>
                                                    <Button
                                                        variant={dir === "rtl" ? "default" : "outline"}
                                                        className="w-full"
                                                        onClick={() =>
                                                            setSettings({
                                                                ...settings,
                                                                dir: "rtl",
                                                            })
                                                        }
                                                    >
                                                        <CircleArrowLeft />
                                                    </Button>
                                                </div>

                                                <div>
                                                    <h3 className="mb-2 text-lg font-medium">Màu nền</h3>
                                                    <div className="grid grid-cols-3 gap-2">
                                                        {themeOptions.map((theme) => (
                                                            <Button
                                                                key={theme.name}
                                                                variant={
                                                                    currentTheme.name === theme.name
                                                                        ? "default"
                                                                        : "outline"
                                                                }
                                                                className={`w-full ${theme.bgColor} ${theme.textColor}`}
                                                                onClick={() =>
                                                                    setSettings({
                                                                        ...settings,
                                                                        currentTheme: theme,
                                                                    })
                                                                }
                                                            ></Button>
                                                        ))}
                                                    </div>
                                                </div>
                                            </AccordionContent>
                                        </AccordionItem>
                                        <AccordionItem value="item-2">
                                            <AccordionTrigger className="text-xl">Độ vừa ảnh</AccordionTrigger>
                                            <AccordionContent>
                                                <h3>Kích Thước Ảnh</h3>
                                                <div className="my-4 flex items-center space-x-2">
                                                    <Checkbox
                                                        id="contain-width"
                                                        checked={containWidth}
                                                        onCheckedChange={(e) =>
                                                            setSettings({
                                                                ...settings,
                                                                containWidth: !!e,
                                                            })
                                                        }
                                                    />
                                                    <label
                                                        htmlFor="contain-width"
                                                        className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                                                    >
                                                        Giới hạn theo chiều rộng
                                                    </label>
                                                </div>
                                                <div className="my-4 flex items-center space-x-2">
                                                    <Checkbox
                                                        id="contain-height"
                                                        checked={containHeight}
                                                        onCheckedChange={(e) =>
                                                            setSettings({
                                                                ...settings,
                                                                containHeight: !!e,
                                                            })
                                                        }
                                                    />
                                                    <label
                                                        htmlFor="contain-height"
                                                        className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                                                    >
                                                        Giới hạn theo chiều cao
                                                    </label>
                                                </div>
                                                <div className="my-4 flex items-center space-x-2">
                                                    <Checkbox
                                                        id="stretch-small"
                                                        checked={stretchSmall}
                                                        onCheckedChange={(e) =>
                                                            setSettings({
                                                                ...settings,
                                                                stretchSmall: !!e,
                                                            })
                                                        }
                                                    />
                                                    <label
                                                        htmlFor="stretch-small"
                                                        className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                                                    >
                                                        Kéo dãn các trang nhỏ
                                                    </label>
                                                </div>

                                                <Separator className="my-4" />
                                                <div className="my-4 mb-2 flex items-center space-x-2">
                                                    <Checkbox
                                                        id="limit-width"
                                                        checked={isLimitWidth}
                                                        disabled={!containWidth}
                                                        onCheckedChange={(e) =>
                                                            setSettings({
                                                                ...settings,
                                                                isLimitWidth: !!e,
                                                            })
                                                        }
                                                    />
                                                    <label
                                                        htmlFor="limit-width"
                                                        className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                                                    >
                                                        Giới hạn chiều rộng tối đa
                                                    </label>
                                                </div>
                                                <div className="flex gap-2">
                                                    <p>{limitWidth}px</p>
                                                    <Slider
                                                        disabled={!isLimitWidth || !containWidth}
                                                        value={[limitWidth]}
                                                        onValueChange={(e) =>
                                                            setSettings({
                                                                ...settings,
                                                                limitWidth: e[0],
                                                            })
                                                        }
                                                        max={window?.outerWidth}
                                                        min={0}
                                                        step={1}
                                                    />
                                                </div>
                                                <div className="my-4 mb-2 flex items-center space-x-2">
                                                    <Checkbox
                                                        id="limit-height"
                                                        checked={isLimitHeight}
                                                        disabled={!containHeight}
                                                        onCheckedChange={(e) =>
                                                            setSettings({
                                                                ...settings,
                                                                isLimitHeight: !!e,
                                                            })
                                                        }
                                                    />
                                                    <label
                                                        htmlFor="limit-height"
                                                        className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                                                    >
                                                        Giới hạn chiều cao tối đa
                                                    </label>
                                                </div>
                                                <div className="flex gap-2">
                                                    <p>{limitHeight}px</p>
                                                    <Slider
                                                        disabled={!isLimitHeight || !containHeight}
                                                        value={[limitHeight]}
                                                        onValueChange={(e) =>
                                                            setSettings({
                                                                ...settings,
                                                                limitHeight: e[0],
                                                            })
                                                        }
                                                        max={window?.outerHeight}
                                                        min={0}
                                                        step={1}
                                                    />
                                                </div>
                                            </AccordionContent>
                                        </AccordionItem>

                                        <AccordionItem value="item-3">
                                            <AccordionTrigger className="text-xl">Hành vi</AccordionTrigger>
                                            <AccordionContent>
                                                <h3>Lật trang bằng cách chạm</h3>
                                                <div className="mb-4 mt-2 flex w-full justify-between gap-1">
                                                    <Button
                                                        variant={tapToTurn === "directional" ? "default" : "outline"}
                                                        className="w-full"
                                                        onClick={() =>
                                                            setSettings({
                                                                ...settings,
                                                                tapToTurn: "directional",
                                                            })
                                                        }
                                                    >
                                                        Theo hướng
                                                    </Button>
                                                    <Button
                                                        className="w-full flex-shrink break-words"
                                                        variant={tapToTurn === "alwaysForward" ? "default" : "outline"}
                                                        onClick={() =>
                                                            setSettings({
                                                                ...settings,
                                                                tapToTurn: "alwaysForward",
                                                            })
                                                        }
                                                    >
                                                        Luôn <br /> tiến tới
                                                    </Button>
                                                    <Button
                                                        variant={tapToTurn === "never" ? "default" : "outline"}
                                                        onClick={() =>
                                                            setSettings({
                                                                ...settings,
                                                                tapToTurn: "never",
                                                            })
                                                        }
                                                        className="w-full"
                                                    >
                                                        Không bao giờ
                                                    </Button>
                                                </div>

                                                <h3>Nhấn đúp để bật/tắt toàn màn hình</h3>
                                                <div className="mb-4 mt-2 flex w-full justify-between gap-1">
                                                    <Button
                                                        variant={doubleTapFs ? "outline" : "default"}
                                                        className="w-full"
                                                        onClick={() =>
                                                            setSettings({
                                                                ...settings,
                                                                doubleTapFs: false,
                                                            })
                                                        }
                                                    >
                                                        Vô hiệu hóa
                                                    </Button>
                                                    <Button
                                                        variant={doubleTapFs ? "default" : "outline"}
                                                        className="w-full"
                                                        onClick={() =>
                                                            setSettings({
                                                                ...settings,
                                                                doubleTapFs: true,
                                                            })
                                                        }
                                                    >
                                                        Kích hoạt
                                                    </Button>
                                                </div>
                                            </AccordionContent>
                                        </AccordionItem>
                                    </Accordion>

                                    <div>
                                        <h3 className="mb-2 mt-2 text-lg font-medium">Tùy chọn khác</h3>
                                        <div className="space-y-4">
                                            <div className="flex items-center space-x-2">
                                                <Checkbox
                                                    id="auto-unlock"
                                                    checked={autoUnlock}
                                                    onCheckedChange={(checked) => setAutoUnlock(checked as boolean)}
                                                />
                                                <Label htmlFor="auto-unlock">Tự động mở khóa chương mới</Label>
                                            </div>
                                            <p className="text-sm text-muted-foreground">
                                                Tự động sử dụng xu để mở khóa chương mới
                                            </p>
                                        </div>
                                    </div>
                                </ScrollArea>

                                <div className="border-t py-4">
                                    <Button onClick={resetSettings} className="w-full">
                                        Thiết lập lại cài đặt
                                    </Button>
                                </div>
                            </SheetContent>
                        </Sheet>
                        <div className="flex space-x-2">
                            <Link href={prevChapterUrl ?? "#"}>
                                <Button
                                    disabled={!prevChapterUrl}
                                    variant="ghost"
                                    size="icon"
                                    className={`${currentTheme.textColor}`}
                                >
                                    <ChevronLeft />
                                </Button>
                            </Link>
                            <Link href={nextChapterUrl ?? "#"}>
                                <Button
                                    disabled={!nextChapterUrl}
                                    variant="ghost"
                                    size="icon"
                                    className={`${currentTheme.textColor}`}
                                >
                                    <ChevronRight />
                                </Button>
                            </Link>
                        </div>
                    </div>
                </div>
            </nav>
        </div>
    );
};

export default ReadComicScreen;

export interface ReadComicScreenProps {
    chapterId: string;
}
