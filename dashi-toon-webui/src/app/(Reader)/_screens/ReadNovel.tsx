"use client";

import { inter, sourceSerif4, merriweatherSans, openSans, nunito, robotoCondensed } from "@/app/fonts";
import React, { useCallback, useEffect, useRef, useState } from "react";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
    AlignCenter,
    AlignJustify,
    AlignLeft,
    AlignRight,
    ChevronLeft,
    ChevronRight,
    LucideIcon,
    Menu,
    Settings,
    ArrowBigUp,
    ArrowBigDown,
} from "lucide-react";
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet";
import { Button } from "@/components/ui/button";
import { Slider } from "@/components/ui/slider";
import Image from "next/image";
import {
    Chapter,
    ChapterContent,
    getChapters,
    getNovelChapterContent,
    getSeriesInfo,
    SeriesResp,
    Volume,
    getVolumes,
    VolumeChapters,
    getUserAccessibleChapters,
} from "@/utils/api/series";
import { toast } from "sonner";
import ChapterMenuList from "../components/ChapterMenuList";
import { UnlockChapter } from "../components/UnlockChapter";
import { useRouter } from "next/navigation";
import { useChapVis } from "@/hooks/useChapVis";
import { useScrollNav } from "@/hooks/useScrollNav";
import Link from "next/link";
import SiteLayout from "@/components/SiteLayout";
import { useInView } from "react-intersection-observer";
import * as env from "@/utils/env";
import CommentDialog from "@/app/(SC_02)/components/CommentDialog";
import { ReportDialog } from "@/components/ReportDialog";
import { reportChapter } from "@/utils/api/reader/report";
import { FaChevronDown } from "react-icons/fa";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";

type TextAlignment = "left" | "center" | "right" | "justify";

type FontOption = {
    name: string;
    className: string;
};

type ThemeOption = {
    name: string;
    bgColor: string;
    textColor: string;
};

const themeOptions: ThemeOption[] = [
    { name: "Dark", bgColor: "bg-neutral-800", textColor: "text-neutral-300" },
    {
        name: "Dark Gray",
        bgColor: "bg-gray-800",
        textColor: "text-neutral-200",
    },
    { name: "White", bgColor: "bg-white", textColor: "text-gray-900" },
    { name: "Cream", bgColor: "bg-[#F5F5DC]", textColor: "text-gray-900" },
    { name: "Light Blue", bgColor: "bg-[#E6F3FF]", textColor: "text-gray-900" },
    { name: "Mint", bgColor: "bg-[#E6FFF2]", textColor: "text-gray-900" },
];

const fontOptions: FontOption[] = [
    { name: "Inter", className: inter.className },
    { name: "Source Serif 4", className: sourceSerif4.className },
    { name: "Merriweather Sans", className: merriweatherSans.className },
    { name: "Open Sans", className: openSans.className },
    { name: "Nunito", className: nunito.className },
    { name: "Roboto Condensed", className: robotoCondensed.className },
];

const STORAGE_KEY = "readerSettings";

type ScrollBehavior = "smooth" | "auto";

interface ReaderSettings {
    textSize: number;
    textAlign: TextAlignment;
    alignmentPx: number;
    currentTheme: ThemeOption;
    currentFont: FontOption;
    scrollAmount: number;
    scrollBehavior: ScrollBehavior;
    showShortcuts: boolean;
    autoUnlock: boolean;
}

interface ChapterWithVolume extends Chapter {
    volumeId: string;
    volumeNumber: number;
}

async function fetchNextChapterUrl(seriesId: string, volumeId: string, chapterId: string, vc: VolumeChapters) {
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

        return `/series/${seriesId}/vol/${nextVol.volumeId}/chap/${firstChapter.id}/novel`;
    }

    return `/series/${seriesId}/vol/${volumeId}/chap/${next.id}/novel`;
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

        return `/series/${seriesId}/vol/${prevVol.volumeId}/chap/${lastChapter.id}/novel`;
    }

    return `/series/${seriesId}/vol/${volumeId}/chap/${prev.id}/novel`;
}

const ReadNovelScreen: React.FC<ReadNovelScreenProps> = ({ volumeId, chapterId, seriesId }) => {
    const router = useRouter();
    const [seriesInfo, setSeriesInfo] = useState<SeriesResp | null>(null);
    const [chapterContent, setChapterContent] = useState<ChapterContent<string> | null>(null);
    const [showUnlock, setShowUnlock] = useState(false);
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [chapters, setChapters] = useState<Chapter[]>([]);
    const [currentChapterNumber, setCurrentChapterNumber] = useState<number>(0);
    const [volumes, setVolumes] = useState<Volume[]>([]);
    const [allChapters, setAllChapters] = useState<ChapterWithVolume[]>([]);
    const [nextChapterUrl, setNextChapterUrl] = useState<string | null>(null);
    const [prevChapterUrl, setPrevChapterUrl] = useState<string | null>(null);
    const [isChapterNumberLoading, setIsChapterNumberLoading] = useState(true);

    const { ref: bottomRef, inView: isBottomVisible } = useInView();
    const hasTrackedView = useRef(false);

    const trackChapterView = useCallback(async () => {
        if (hasTrackedView.current) return;
        try {
            const response = await fetch(`${env.getBackendHost()}/api/Chapters/${chapterId}/analytics/view`, {
                method: "POST",
            });
            if (response.ok) {
                hasTrackedView.current = true;
                console.log("Chapter view tracked");
            } else {
                console.error("Failed to track chapter view:", response.status);
            }
        } catch (error) {
            console.error("Failed to track chapter view:", error);
        }
    }, [chapterId]);

    useEffect(() => {
        if (!chapterContent) return;

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

            // Check if we've reached 20 seconds (20000 milliseconds)
            if (timeSpent >= 20000) {
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
    }, [chapterContent, trackChapterView]);

    useEffect(() => {
        if (isBottomVisible) {
            trackChapterView();
        }
    }, [isBottomVisible, trackChapterView]);

    useChapVis(seriesId, chapterId, false);
    useEffect(() => {
        const fetchChapters = async () => {
            const [chaptersData, error] = await getChapters(seriesId, volumeId);
            if (error) {
                console.error("Error fetching chapters:", error);
                toast.error("Lỗi tải truyện!");
                return;
            }

            if (chaptersData) {
                const sortedChapters = [...chaptersData].sort((a, b) => a.chapterNumber - b.chapterNumber);
                setChapters(sortedChapters);
            }
        };

        fetchChapters();
    }, [seriesId, volumeId, chapterId]);

    useEffect(() => {
        async function work() {
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

        work();
    }, [chapterId, seriesId, volumeId]);

    useEffect(() => {
        const fetchSeriesInfo = async () => {
            const [data, error] = await getSeriesInfo(seriesId);
            if (error) {
                console.error("Error fetching series info:", error);
                toast.error("Failed to fetch series information");
            } else if (data) {
                setSeriesInfo(data);
            }
        };

        fetchSeriesInfo();
    }, [seriesId]);

    const fetchChapterContent = React.useCallback(async () => {
        const [data, error] = await getNovelChapterContent(seriesId, volumeId, chapterId);
        if (error) {
            if (error.status === 401) {
                setShowUnlock(true);
                setIsAuthenticated(false);
            } else if (error.status === 403) {
                setShowUnlock(true);
                setIsAuthenticated(true);
            } else {
                console.error("Error fetching chapter content:", error);
                toast.error("Lỗi khi lấy nội dung truyện! Thử lại sau!");
            }
        } else if (data) {
            setChapterContent(data);
        }
    }, [seriesId, volumeId, chapterId]);

    useEffect(() => {
        fetchChapterContent();
    }, [fetchChapterContent]);

    const showNav = useScrollNav();

    const getStoredValue = <T,>(key: keyof ReaderSettings, defaultValue: T): T => {
        try {
            if (typeof window !== "undefined") {
                const savedSettings = localStorage.getItem(STORAGE_KEY);
                if (savedSettings) {
                    const settings = JSON.parse(savedSettings);
                    if (settings[key] !== undefined && settings[key] !== null) {
                        return settings[key];
                    }
                }
            }
        } catch (error) {
            console.error("Error reading from localStorage:", error);
        }
        return defaultValue;
    };

    const [textSize, setTextSize] = useState(() => getStoredValue("textSize", 16));

    const [textAlign, setTextAlign] = useState<TextAlignment>(() =>
        getStoredValue("textAlign", "left" as TextAlignment),
    );

    const [alignmentPx, setAlignmentPx] = useState(() => getStoredValue("alignmentPx", 0));

    const [currentTheme, setCurrentTheme] = useState<ThemeOption>(() =>
        getStoredValue("currentTheme", themeOptions[0]),
    );

    const [currentFont, setCurrentFont] = useState<FontOption>(() => getStoredValue("currentFont", fontOptions[0]));

    const [scrollAmount, setScrollAmount] = useState(() => getStoredValue("scrollAmount", 50));

    const [scrollBehavior, setScrollBehavior] = useState<ScrollBehavior>(() =>
        getStoredValue("scrollBehavior", "smooth"),
    );

    const [showShortcuts, setShowShortcuts] = useState(() => getStoredValue("showShortcuts", true));

    const [autoUnlock, setAutoUnlock] = useState(() => getStoredValue("autoUnlock", false));

    const [isContentHovered, setIsContentHovered] = useState(false);
    const contentAreaRef = useRef<HTMLDivElement>(null);

    const handleKeyPress = useCallback(
        (e: KeyboardEvent) => {
            // Check if the event target is within the content area or if content area is hovered
            const isContentArea = contentAreaRef.current?.contains(e.target as Node) || isContentHovered;
            if (!isContentArea) return;

            if (e.key === "ArrowLeft" && prevChapterUrl) {
                router.push(prevChapterUrl);
            } else if (e.key === "ArrowRight" && nextChapterUrl) {
                router.push(nextChapterUrl);
            } else if (e.key === " " || e.key === "ArrowDown") {
                window.scrollBy({ top: window.innerHeight * (scrollAmount / 100), behavior: scrollBehavior });
                e.preventDefault();
            } else if (e.key === "ArrowUp") {
                window.scrollBy({ top: -window.innerHeight * (scrollAmount / 100), behavior: scrollBehavior });
                e.preventDefault();
            }
        },
        [nextChapterUrl, prevChapterUrl, scrollAmount, scrollBehavior, router, isContentHovered],
    );

    const handleSideClick = (direction: "up" | "down") => {
        const amount = window.innerHeight * (scrollAmount / 100);
        window.scrollBy({
            top: direction === "up" ? -amount : amount,
            behavior: scrollBehavior,
        });
    };

    const scrollToTop = () => {
        window.scrollTo({ top: 0, behavior: scrollBehavior });
    };

    useEffect(() => {
        window.addEventListener("keydown", handleKeyPress);
        return () => window.removeEventListener("keydown", handleKeyPress);
    }, [handleKeyPress]);

    const getTextStyles = () => {
        const lineHeight = Math.max(Math.round(textSize * 1.5), textSize + 8);

        return {
            fontSize: `${textSize}px`,
            lineHeight: `${lineHeight}px`,
            textAlign,
            "& > *": {
                fontSize: "inherit !important",
                lineHeight: "inherit !important",
                textAlign: "inherit !important",
            },
        };
    };

    // const getContainerStyles = () => {
    //     return {
    //         marginLeft: `${alignmentPx}px`,
    //         marginRight: `${alignmentPx}px`,
    //     };
    // };

    const alignmentOptions = [
        { icon: AlignLeft, value: "left" },
        { icon: AlignCenter, value: "center" },
        { icon: AlignRight, value: "right" },
        { icon: AlignJustify, value: "justify" },
    ] satisfies { icon: LucideIcon; value: TextAlignment }[];

    useEffect(() => {
        try {
            const settings: ReaderSettings = {
                textSize,
                textAlign,
                alignmentPx,
                currentTheme,
                currentFont,
                scrollAmount,
                scrollBehavior,
                showShortcuts,
                autoUnlock,
            };
            localStorage.setItem(STORAGE_KEY, JSON.stringify(settings));
        } catch (error) {
            console.error("Error saving to localStorage:", error);
        }
    }, [
        textSize,
        textAlign,
        alignmentPx,
        currentTheme,
        currentFont,
        scrollAmount,
        scrollBehavior,
        showShortcuts,
        autoUnlock,
    ]);

    const resetSettings = () => {
        localStorage.removeItem(STORAGE_KEY);
        setTextSize(16);
        setTextAlign("left");
        setAlignmentPx(0);
        setCurrentTheme(themeOptions[0]);
        setCurrentFont(fontOptions[0]);
        setScrollAmount(30);
        setScrollBehavior("smooth");
        setAutoUnlock(false);
    };

    const contentRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const contentElement = contentRef.current;
        if (!contentElement) return;

        const preventCopy = (e: Event) => {
            e.preventDefault();
            toast.error("Don't copy! Bad user! 💢");
        };

        contentElement.addEventListener("copy", preventCopy);
        contentElement.addEventListener("cut", preventCopy);

        return () => {
            contentElement.removeEventListener("copy", preventCopy);
            contentElement.removeEventListener("cut", preventCopy);
        };
    }, []);

    const preventContextMenu = (e: React.MouseEvent) => {
        e.preventDefault();
    };

    const NavigationButtons = () => (
        <div className="flex space-x-2">
            <Button
                variant="ghost"
                size="icon"
                className={`${currentTheme.textColor}`}
                disabled={!prevChapterUrl}
                asChild
            >
                <Link href={prevChapterUrl ?? "#"}>
                    <ChevronLeft />
                </Link>
            </Button>
            <Button
                variant="ghost"
                size="icon"
                className={`${currentTheme.textColor}`}
                disabled={!nextChapterUrl}
                asChild
            >
                <Link href={nextChapterUrl ?? "#"}>
                    <ChevronRight />
                </Link>
            </Button>
        </div>
    );

    const [mounted, setMounted] = useState(false);

    useEffect(() => {
        setMounted(true);
    }, []);

    useEffect(() => {
        const fetchAllChapters = async () => {
            setIsChapterNumberLoading(true);
            const [volumesData, volumesError] = await getVolumes(seriesId);
            if (volumesError) {
                console.error("Error fetching volumes:", volumesError);
                setIsChapterNumberLoading(false); // End loading on error
                return;
            }

            if (volumesData) {
                setVolumes(volumesData);

                // Fetch chapters for each volume
                const allChaptersPromises = volumesData.map(async (volume) => {
                    const [chaptersData, error] = await getChapters(seriesId, volume.volumeId.toString());
                    if (error) {
                        console.error(`Error fetching chapters for volume ${volume.volumeId}:`, error);
                        return [];
                    }
                    return chaptersData.map((chapter) => ({
                        ...chapter,
                        volumeId: volume.volumeId.toString(),
                        volumeNumber: volume.volumeNumber,
                    }));
                });

                const chaptersFromAllVolumes = (await Promise.all(allChaptersPromises)).flat();
                const sortedChapters = chaptersFromAllVolumes.sort((a, b) => {
                    if (a.volumeNumber !== b.volumeNumber) {
                        return a.volumeNumber - b.volumeNumber;
                    }
                    return a.chapterNumber - b.chapterNumber;
                });
                setAllChapters(sortedChapters);

                // Find current chapter index
                const currentIndex = sortedChapters.findIndex((ch) => ch.id.toString() === chapterId);
                if (currentIndex !== -1) {
                    setCurrentChapterNumber(currentIndex);
                }
            }
            setIsChapterNumberLoading(false);
        };

        fetchAllChapters();
    }, [seriesId, chapterId]);

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

    if (!mounted) {
        return null;
    }

    return (
        <SiteLayout>
            <>
                <div
                    className={`min-h-screen transition-colors duration-300 ${currentTheme.bgColor}`}
                    onContextMenu={preventContextMenu}
                >
                    <div className="flex justify-center px-4">
                        <div className={`container mx-auto w-full md:max-w-6xl ${currentTheme.textColor}`}>
                            <header className="flex items-center justify-between gap-4 py-4">
                                <div className="flex min-w-0 items-center space-x-4">
                                    <Link
                                        href={`/series/${seriesId}`}
                                        className={`flex min-w-0 items-center gap-2 ${currentTheme.textColor} hover:opacity-80`}
                                    >
                                        <ChevronLeft className="h-5 w-5 shrink-0" />
                                        {seriesInfo && (
                                            <>
                                                <Image
                                                    src={seriesInfo.thumbnail || `https://placehold.co/225x225.png`}
                                                    alt={seriesInfo.title}
                                                    className="rounded-lg object-cover"
                                                    width={48}
                                                    height={48}
                                                />
                                                <h1 className="truncate text-xl font-bold">{seriesInfo.title}</h1>
                                            </>
                                        )}
                                    </Link>
                                </div>
                                <NavigationButtons />
                            </header>
                            <hr className="border-gray-line-base absolute left-1/2 w-screen -translate-x-1/2 lg:w-[1024px]"></hr>

                            <main className={`select-none break-words py-6`}>
                                {showUnlock ? (
                                    <div className="relative min-h-[calc(100vh-200px)]">
                                        <UnlockChapter
                                            seriesId={seriesId}
                                            volumeId={volumeId}
                                            chapterId={chapterId}
                                            isAuthenticated={isAuthenticated}
                                            timeUntilFree="22h : 35m : 01"
                                            onUnlockSuccess={() => {
                                                setShowUnlock(false);
                                                fetchChapterContent();
                                            }}
                                            autoUnlock={autoUnlock}
                                            onAutoUnlockChange={(checked) => setAutoUnlock(checked)}
                                        />
                                    </div>
                                ) : (
                                    chapterContent && (
                                        <>
                                            <h2 className="mb-4 flex justify-start text-2xl font-bold">
                                                {isChapterNumberLoading ? (
                                                    <span className="animate-pulse">Chương : ...</span>
                                                ) : (
                                                    `Chương ${currentChapterNumber}: ${chapterContent?.title}`
                                                )}
                                            </h2>
                                            <div className="relative">
                                                {/* Content area with hover detection */}
                                                <div
                                                    ref={contentAreaRef}
                                                    className="relative"
                                                    onMouseEnter={() => setIsContentHovered(true)}
                                                    onMouseLeave={() => setIsContentHovered(false)}
                                                >
                                                    {/* Left click area */}
                                                    <div
                                                        className="fixed left-0 top-0 h-screen w-1/4 cursor-pointer"
                                                        onClick={() => handleSideClick("up")}
                                                    />

                                                    {/* Right click area */}
                                                    <div
                                                        className="fixed right-0 top-0 h-screen w-1/4 cursor-pointer"
                                                        onClick={() => handleSideClick("down")}
                                                    />

                                                    {/* Content */}
                                                    <div
                                                        ref={contentRef}
                                                        className={`prose prose-neutral lg:prose-xl !${currentTheme.textColor} override-content`}
                                                        style={{
                                                            ...getTextStyles(),
                                                            color: "inherit",
                                                        }}
                                                        dangerouslySetInnerHTML={{
                                                            __html: chapterContent.content,
                                                        }}
                                                    />

                                                    {/* Navigation buttons at bottom */}
                                                    <div className="mt-8 flex items-center justify-between">
                                                        <Button
                                                            variant="outline"
                                                            disabled={!prevChapterUrl}
                                                            className={currentTheme.textColor}
                                                        >
                                                            <Link href={nextChapterUrl ?? "#"}>Chương trước</Link>
                                                        </Button>

                                                        <Button
                                                            variant="outline"
                                                            onClick={scrollToTop}
                                                            className={currentTheme.textColor}
                                                        >
                                                            <ArrowBigUp className="mr-2 h-4 w-4" />
                                                            Lên đầu trang
                                                        </Button>

                                                        <Button
                                                            variant="outline"
                                                            disabled={!nextChapterUrl}
                                                            className={currentTheme.textColor}
                                                        >
                                                            <Link href={nextChapterUrl ?? "#"}>Chương sau</Link>
                                                        </Button>
                                                    </div>

                                                    {showShortcuts && (
                                                        <div className="fixed bottom-20 right-4 rounded-lg bg-black/80 p-4 text-sm text-white opacity-30 transition-opacity hover:opacity-100">
                                                            <div className="mb-2 flex items-center justify-between">
                                                                <p>Phím tắt:</p>
                                                                <button
                                                                    onClick={() => setShowShortcuts(false)}
                                                                    className="text-white/80 hover:text-white"
                                                                    aria-label="Close shortcuts"
                                                                >
                                                                    <FaChevronDown
                                                                        className="text-white"
                                                                        aria-label="Close shortcuts"
                                                                    />
                                                                </button>
                                                            </div>
                                                            <ul className="list-inside list-disc">
                                                                <li className="hidden md:list-item">
                                                                    ← → : Chương trước/sau
                                                                </li>
                                                                <li className="hidden md:list-item">
                                                                    ↑ ↓ : Cuộn lên/xuống
                                                                </li>
                                                                <li className="hidden md:list-item">
                                                                    Space : Cuộn xuống
                                                                </li>
                                                                <li className="md:hidden">Chạm trái/phải để cuộn</li>
                                                                <li className="md:hidden">Vuốt lên/xuống để cuộn</li>
                                                            </ul>
                                                        </div>
                                                    )}

                                                    {!showShortcuts && (
                                                        <button
                                                            onClick={() => setShowShortcuts(true)}
                                                            className="fixed bottom-20 right-4 rounded-lg bg-black/80 p-2 text-sm text-white opacity-30 transition-opacity hover:opacity-100"
                                                            aria-label="Show shortcuts"
                                                        >
                                                            ⌨️
                                                        </button>
                                                    )}
                                                </div>
                                            </div>
                                        </>
                                    )
                                )}
                            </main>
                        </div>
                    </div>

                    <nav
                        className={`fixed bottom-0 left-0 right-0 border-t ${currentTheme.bgColor} transition-transform duration-300 ${showNav ? "translate-y-0" : "translate-y-full"}`}
                    >
                        <div className="mx-auto max-w-6xl px-4">
                            <div className="flex h-16 items-center justify-between">
                                <Sheet>
                                    <SheetTrigger asChild>
                                        <Button variant="ghost" size="icon" className={`${currentTheme.textColor}`}>
                                            <Menu />
                                        </Button>
                                    </SheetTrigger>
                                    <ChapterMenuList
                                        seriesId={seriesId}
                                        volumeId={volumeId}
                                        chapterId={chapterId}
                                        type="novel"
                                    />
                                </Sheet>

                                <div className="flex flex-1 items-center px-4">
                                    <p className="mr-2 text-sm font-bold">
                                        {isChapterNumberLoading ? (
                                            <span className="animate-pulse">Chương : ...</span>
                                        ) : (
                                            `Chương ${currentChapterNumber}: ${chapterContent?.title}`
                                        )}
                                    </p>
                                    <ReportDialog
                                        contentId={seriesId}
                                        contentType="series"
                                        contentTitle={seriesInfo?.title}
                                        onReport={handleReport}
                                        iconSize={4}
                                    />
                                </div>
                                <CommentDialog buttonVariant="icon" chapterId={chapterId} currentTheme={currentTheme} />

                                <Sheet>
                                    <SheetTrigger asChild>
                                        <Button variant="ghost" size="icon" className={`${currentTheme.textColor}`}>
                                            <Settings />
                                        </Button>
                                    </SheetTrigger>
                                    <SheetContent side="right">
                                        <div className="flex h-full flex-col">
                                            <ScrollArea className="flex-grow pr-4">
                                                <div className="space-y-6">
                                                    <div>
                                                        <h3 className="mb-2 text-lg font-medium">Phông chữ</h3>
                                                        <div className="grid grid-cols-2 gap-2">
                                                            {fontOptions.map((font) => (
                                                                <Button
                                                                    key={font.name}
                                                                    variant={
                                                                        currentFont.name === font.name
                                                                            ? "default"
                                                                            : "outline"
                                                                    }
                                                                    className={`w-full ${font.className}`}
                                                                    onClick={() => setCurrentFont(font)}
                                                                >
                                                                    {font.name}
                                                                </Button>
                                                            ))}
                                                        </div>
                                                    </div>

                                                    <div className="flex items-center justify-between">
                                                        <h3 className="text-lg font-medium">Kích thước chữ</h3>
                                                        <div className="flex items-center space-x-2">
                                                            <Button
                                                                variant="outline"
                                                                size="icon"
                                                                onClick={() => setTextSize(Math.max(10, textSize - 2))}
                                                            >
                                                                -
                                                            </Button>
                                                            <span className="w-12 text-center">{textSize}</span>
                                                            <Button
                                                                variant="outline"
                                                                size="icon"
                                                                onClick={() => setTextSize(Math.min(48, textSize + 2))}
                                                            >
                                                                +
                                                            </Button>
                                                        </div>
                                                    </div>

                                                    <div>
                                                        <h3 className="mb-2 text-lg font-medium">Căn chỉnh</h3>
                                                        <div className="space-y-4">
                                                            <div className="flex justify-between">
                                                                {alignmentOptions.map(({ icon: Icon, value }) => (
                                                                    <Button
                                                                        key={value}
                                                                        variant={
                                                                            textAlign === value ? "default" : "outline"
                                                                        }
                                                                        size="icon"
                                                                        onClick={() => setTextAlign(value)}
                                                                    >
                                                                        <Icon className="h-4 w-4" />
                                                                    </Button>
                                                                ))}
                                                            </div>
                                                            <div>
                                                                <div className="mb-2 flex justify-between">
                                                                    <span className="text-sm font-medium">Lề</span>
                                                                    <span className="text-sm text-muted-foreground">
                                                                        {alignmentPx}
                                                                        px
                                                                    </span>
                                                                </div>
                                                                <Slider
                                                                    value={[alignmentPx]}
                                                                    onValueChange={([value]) => setAlignmentPx(value)}
                                                                    max={100}
                                                                    step={5}
                                                                />
                                                            </div>
                                                        </div>
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
                                                                    onClick={() => setCurrentTheme(theme)}
                                                                ></Button>
                                                            ))}
                                                        </div>
                                                    </div>

                                                    <div>
                                                        <h3 className="mb-2 text-lg font-medium">Cài đặt cuộn</h3>
                                                        <div className="space-y-4">
                                                            <div>
                                                                <div className="mb-2 flex justify-between">
                                                                    <span className="text-sm font-medium">
                                                                        Khoảng cách cuộn
                                                                    </span>
                                                                    <span className="text-sm text-muted-foreground">
                                                                        {scrollAmount}%
                                                                    </span>
                                                                </div>
                                                                <Slider
                                                                    value={[scrollAmount]}
                                                                    onValueChange={([value]) => setScrollAmount(value)}
                                                                    min={10}
                                                                    max={100}
                                                                    step={10}
                                                                />
                                                            </div>
                                                            <div>
                                                                <h4 className="mb-2 text-sm font-medium">Kiểu cuộn</h4>
                                                                <div className="flex gap-2">
                                                                    <Button
                                                                        variant={
                                                                            scrollBehavior === "smooth"
                                                                                ? "default"
                                                                                : "outline"
                                                                        }
                                                                        onClick={() => setScrollBehavior("smooth")}
                                                                        className="flex-1"
                                                                    >
                                                                        Mượt
                                                                    </Button>
                                                                    <Button
                                                                        variant={
                                                                            scrollBehavior === "auto"
                                                                                ? "default"
                                                                                : "outline"
                                                                        }
                                                                        onClick={() => setScrollBehavior("auto")}
                                                                        className="flex-1"
                                                                    >
                                                                        Tức thì
                                                                    </Button>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>

                                                    <div>
                                                        <h3 className="mb-2 text-lg font-medium">Tùy chọn khác</h3>
                                                        <div className="space-y-4">
                                                            <div className="flex items-center space-x-2">
                                                                <Checkbox
                                                                    id="auto-unlock"
                                                                    checked={autoUnlock}
                                                                    onCheckedChange={(checked) =>
                                                                        setAutoUnlock(checked as boolean)
                                                                    }
                                                                />
                                                                <Label htmlFor="auto-unlock">
                                                                    Tự động mở khóa chương mới
                                                                </Label>
                                                            </div>
                                                            <p className="text-sm text-muted-foreground">
                                                                Tự động sử dụng xu để mở khóa chương mới
                                                            </p>
                                                        </div>
                                                    </div>
                                                </div>
                                            </ScrollArea>
                                            <div className="border-t py-4">
                                                <Button onClick={resetSettings} className="w-full">
                                                    Đặt lại cài đặt
                                                </Button>
                                            </div>
                                        </div>
                                    </SheetContent>
                                </Sheet>
                                <NavigationButtons />
                            </div>
                        </div>
                    </nav>
                </div>
                {/* <ChapterComment
                    bgColor={currentTheme.bgColor}
                    textColor={currentTheme.textColor}
                    chapterId={chapterId}
                /> */}
            </>
        </SiteLayout>
    );
};

export default ReadNovelScreen;

export interface ReadNovelScreenProps {
    seriesId: string;
    chapterId: string;
    volumeId: string;
}
