"use client";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import Image from "next/image";
import React, { useEffect, useState } from "react";
import { FaEye } from "react-icons/fa";
import { FaRegSmile } from "react-icons/fa";
import cx from "classnames";

import { FaRegHeart } from "react-icons/fa";
import { FaHeart } from "react-icons/fa";
import { FaRegClock } from "react-icons/fa";
import { FaShareAlt } from "react-icons/fa";
import { vi } from "date-fns/locale/vi";
import { RATING_CONFIG_2, STATUS_CONFIG_2 } from "@/utils/consts";
import {
    Analytics,
    Chapter,
    getChapters,
    getUserAccessibleChapters,
    getVolumes,
    SeriesResp,
    Volume,
    VolumeChapters,
} from "@/utils/api/series";
import { toast } from "sonner";
import { add, formatDistanceToNow, set } from "date-fns";
import Link from "next/link";
import { Skeleton } from "@/components/ui/skeleton";
import {
    FollowedSeries,
    followSeries,
    getFollowedSeriesById,
    getFollowedSeriesById2,
    unfollowSeries,
} from "@/utils/api/user";
import * as gtag from "@/utils/gtag";
import { BsDot } from "react-icons/bs";
import { useLocalStorage } from "usehooks-ts";
import { useRouter } from "next/navigation";
import { ReportDialog } from "@/components/ReportDialog";
import { reportSeries } from "@/utils/api/reader/report";

export interface SeriesInfoProps {
    seriesInfo?: SeriesResp;
    analytics?: Analytics;
}

export default function SeriesInfo(props: SeriesInfoProps) {
    const router = useRouter();
    const { seriesInfo, analytics } = props;
    const [session] = useLocalStorage("session", null);
    const [isFollowing, setIsFollowing] = useState(false);
    const [vc, setVc] = useState<VolumeChapters>();
    const [readLink, setReadLink] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [buttonText, setButtonText] = useState("Bắt đầu đọc");

    useEffect(() => {
        async function work() {
            if (!seriesInfo) {
                return;
            }
            setIsLoading(true);
            const [data, err] = await getUserAccessibleChapters(seriesInfo.id.toString());

            if (err) {
                toast.error("Không thể tải được tập truyện");
                setIsLoading(false);
                return;
            }

            setVc(data);
            const seriesType = seriesInfo.type === 1 ? "novel" : "comic";

            // Check for continued reading first
            const [dataF, errF] = await getFollowedSeriesById2(seriesInfo.id);
            if (dataF?.isFollowed && dataF.detail.latestChapterReadId !== null) {
                const readLink = `/series/${seriesInfo.id}/vol/${dataF.detail.latestVolumeReadId}/chap/${dataF.detail.latestChapterReadId}/${seriesType}`;
                setReadLink(readLink);
                setButtonText("Tiếp tục đọc");
                setIsLoading(false);
                return;
            }

            // Check for first available chapter
            if (data.firstChap && data.firstChapVol) {
                const readLink = `/series/${seriesInfo.id}/vol/${data.firstChapVol.volumeId}/chap/${data.firstChap.id}/${seriesType}`;
                setReadLink(readLink);
                setButtonText("Bắt đầu đọc"); // Set to "Bắt đầu đọc" if there are chapters available
            } else {
                setReadLink(null);
                setButtonText("Không có chương nào"); // Only set this if there are no chapters
            }

            setIsLoading(false);
        }

        work();
    }, [seriesInfo]);

    useEffect(() => {
        async function fetchFollowedStatus() {
            if (!seriesInfo) return;
            const [data, err] = await getFollowedSeriesById(seriesInfo.id);
            if (err) {
                return;
            }
            setIsFollowing(data);
        }
        fetchFollowedStatus();
    }, [seriesInfo]);

    const handleFollow = async () => {
        if (!seriesInfo) return;

        if (!session) {
            toast.warning("Vui lòng đăng nhập để theo dõi series");
            router.push("/login?returnUrl=" + window.location.pathname + window.location.search);
            return;
        }

        try {
            if (isFollowing) {
                const [_, err] = await unfollowSeries(seriesInfo.id);
                if (err) {
                    toast.error("Không thể bỏ theo dõi series");
                    return;
                }
                toast.success("Đã bỏ theo dõi series");
            } else {
                const [_, err] = await followSeries(seriesInfo.id);
                if (err) {
                    toast.error("Không thể theo dõi series");
                    return;
                }
                toast.success("Đã theo dõi series");
            }
            setIsFollowing(!isFollowing);
        } catch (error) {
            toast.error("Đã xảy ra lỗi");
        }
    };

    const handleReport = async (
        contentId: string,
        reason: string,
        contentType: "series" | "chapter" | "review" | "comment",
    ) => {
        try {
            const [error] = await reportSeries(parseInt(contentId), reason);
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

    if (isLoading) {
        return <SeriesInfoSkeleton />;
    }

    return (
        <div className="flex flex-wrap justify-center gap-4 lg:flex-nowrap">
            <div className="flex-shrink-0">
                <Image
                    src={seriesInfo?.thumbnail ?? "/images/placeholder.png"}
                    width={400}
                    height={533}
                    alt="Ảnh bìa Series"
                    className="w-[400px] object-contain lg:rounded-md"
                />
            </div>

            <div className="flex flex-grow flex-col">
                <div className="flex items-center gap-1">
                    <Badge>{seriesInfo?.type === 1 ? "Tiểu thuyểt" : "Truyện tranh"}</Badge>
                    <BsDot style={{ scale: 1.5 }} />
                    <Badge className={cx(RATING_CONFIG_2[1].color)}>
                        {RATING_CONFIG_2[seriesInfo?.contentRating ?? 1].label}
                    </Badge>
                </div>
                <div className="flex items-center gap-4">
                    <h1 className="flex-shrink text-3xl font-bold">{seriesInfo?.title}</h1>
                </div>
                <p className="mb-1">Tác giả: {seriesInfo?.author}</p>
                <p className="mb-1 flex items-center gap-2">
                    Tình trạng:{" "}
                    <span className="flex items-center gap-2">
                        <span className={cx("h-2 w-2 rounded-full", STATUS_CONFIG_2[seriesInfo?.status ?? 1].color)} />
                        <span className="text-sm font-medium">{STATUS_CONFIG_2[seriesInfo?.status ?? 1].label}</span>
                    </span>
                </p>

                <div className="mb-1 flex flex-wrap gap-2">
                    <p>Thể loại:</p>
                    <li className="flex flex-wrap gap-2">
                        {seriesInfo?.genres.map((genre) => <Badge key={genre.id}>{genre.title}</Badge>)}
                    </li>
                </div>

                <div className="relative aspect-[32/9] w-full lg:mt-auto">
                    <Image
                        src={"/images/dashifan-banner.png"}
                        fill={true}
                        alt="Ảnh bìa Series"
                        className="object-cover"
                    />
                </div>

                <ul className="grid grid-cols-[repeat(auto-fit,minmax(135px,1fr))] justify-between gap-2 py-4">
                    <li className="flex flex-grow flex-col items-center gap-2 py-6">
                        <FaRegSmile className="me-1 text-green-500" size={40} />
                        <p className="text-center">
                            {Math.round(analytics?.rating ?? 100)}% / {analytics?.reviewCount ?? 0} đánh giá
                        </p>
                    </li>

                    <li className="flex flex-grow flex-col items-center gap-2 py-6">
                        <FaHeart className="me-1 text-pink-500" size={40} />
                        <p className="text-center">{analytics?.followCount ?? 0} người theo dõi</p>
                    </li>

                    <li className="flex flex-grow flex-col items-center gap-2 py-6">
                        <FaEye className="me-1" size={40} />
                        <p className="text-center">{analytics?.viewCount ?? 0} lượt xem</p>
                    </li>

                    <li className="flex flex-grow flex-col items-center gap-2 py-6">
                        <FaRegClock className="me-1" size={40} />
                        <p className="text-center">
                            {analytics?.lastModified
                                ? formatDistanceToNow(Date.parse(analytics.lastModified), {
                                      locale: vi,
                                      addSuffix: false,
                                  })
                                : "Không có"}{" "}
                            trước
                        </p>
                    </li>
                </ul>

                <div className="flex flex-wrap justify-end gap-1 lg:gap-2">
                    <Link href={readLink ?? "#"} className="flex-grow">
                        <Button
                            disabled={!readLink}
                            className={cx("w-full bg-blue-600 px-24 text-white hover:bg-blue-700")}
                        >
                            {buttonText}
                        </Button>
                    </Link>
                    <FollowButton
                        isFollowing={isFollowing}
                        followCount={analytics?.followCount}
                        onClick={handleFollow}
                    />
                    <Button
                        variant={"ghost"}
                        className="flex w-auto flex-grow-0 items-center gap-2"
                        onClick={(e) => {
                            navigator.clipboard.writeText(window.location.href);

                            gtag.eventShare(
                                seriesInfo?.id?.toString() ?? "",
                                seriesInfo?.type === 1 ? "NOVEL" : "COMIC",
                            );
                            toast.success("Liên kết đã được sao chép vào clipboard");
                        }}
                    >
                        <FaShareAlt /> Chia sẻ
                    </Button>
                    <ReportDialog
                        contentId={seriesInfo?.id?.toString() ?? ""}
                        contentType="series"
                        contentTitle={seriesInfo?.title}
                        onReport={handleReport}
                    />
                </div>
            </div>
        </div>
    );
}

function SeriesInfoSkeleton() {
    return (
        <div className="flex flex-wrap justify-center gap-4 lg:flex-nowrap">
            <div className="flex-shrink-0">
                <Skeleton className="h-[533px] w-[400px] lg:rounded-md" />
            </div>

            <div className="flex flex-grow flex-col">
                <div className="flex items-center gap-4">
                    <Skeleton className="h-9 w-64" /> {/* Title */}
                    <Skeleton className="h-6 w-24" /> {/* Badge */}
                </div>
                <Skeleton className="mb-1 mt-2 h-6 w-48" /> {/* Author */}
                <Skeleton className="mb-1 h-6 w-36" /> {/* Status */}
                <div className="mb-1 flex flex-wrap gap-2">
                    <Skeleton className="h-6 w-24" /> {/* Genre label */}
                    <div className="flex gap-2">
                        {[1, 2, 3].map((i) => (
                            <Skeleton key={i} className="h-6 w-20" />
                        ))}
                    </div>
                </div>
                <div className="relative mt-4 aspect-[32/9] w-full">
                    <Skeleton className="h-full w-full" />
                </div>
                <ul className="grid grid-cols-[repeat(auto-fit,minmax(135px,1fr))] justify-between gap-2 py-4">
                    {[1, 2, 3, 4].map((i) => (
                        <li key={i} className="flex flex-col items-center gap-2 py-6">
                            <Skeleton className="h-10 w-10" />
                            <Skeleton className="h-6 w-24" />
                        </li>
                    ))}
                </ul>
                <div className="mt-auto flex flex-wrap justify-end gap-1 lg:gap-2">
                    <Skeleton className="h-10 flex-grow" /> {/* Read button */}
                    <Skeleton className="h-10 w-20" /> {/* Follow button */}
                    <Skeleton className="h-10 w-24" /> {/* Share button */}
                </div>
            </div>
        </div>
    );
}

function FollowButton({
    isFollowing,
    followCount,
    onClick,
}: {
    isFollowing: boolean;
    followCount: number | undefined;
    onClick?: () => void;
}) {
    return (
        <Button
            onClick={onClick}
            className={cx(
                "flex w-auto flex-grow-0 items-center gap-2 border border-transparent bg-transparent text-white transition-colors",
                {
                    "bg-pink-500 hover:bg-pink-600": isFollowing,
                    "border-pink-500 hover:bg-pink-900": !isFollowing,
                },
            )}
        >
            {isFollowing ? <FaHeart /> : <FaRegHeart />}
        </Button>
    );
}
