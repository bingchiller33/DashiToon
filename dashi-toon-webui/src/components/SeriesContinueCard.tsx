/* eslint-disable @next/next/no-img-element */
import { CarouselItem } from "@/components/ui/carousel";
import { Badge } from "@/components/ui/badge";
import React from "react";
import Link from "next/link";
import { ThumbsUp } from "lucide-react";
import { STATUS_CONFIG_2 } from "@/utils/consts";
import cx from "classnames";
import { SeriesResp, SeriesResp2 } from "@/utils/api/series";
import useUwU from "@/hooks/useUwU";
import { FollowedSeries } from "@/utils/api/user";
import { Progress } from "./ui/progress";

export const placeholder = "/images/atg.webp";

const getContinueReadingLink = (series: FollowedSeries) => {
    if (series.latestVolumeReadId === null || series.latestChapterReadId === null) {
        return `/series/${series.seriesId}`;
    }
    return `/series/${series.seriesId}/vol/${series.latestVolumeReadId}/chap/${series.latestChapterReadId}/${series.type === 1 ? "novel" : "comic"}`;
};

export default function SeriesContinueCard(props: { data: FollowedSeries }) {
    const [uwu] = useUwU();
    const img = uwu ? placeholder : props.data.thumbnail;

    return (
        <Link href={getContinueReadingLink(props.data)} className="group overflow-hidden text-sm">
            <div className="relative">
                <img className="aspect-[3/4] w-full" src={img} alt="Ảnh thu nhỏ" />

                <p className="absolute right-0 top-0 bg-black/50 p-1">
                    {props.data?.type === 2 ? "Truyện tranh" : "Tiểu thuyết"}
                </p>
                <div className="absolute bottom-0 max-h-full w-full translate-y-full bg-black/50 from-transparent to-black/60 pb-2 transition-all group-hover:translate-y-0">
                    <div className="-translate-y-full bg-gradient-to-b from-transparent via-black/40 via-20% to-black/60 p-2 pb-1 pt-8 transition-all group-hover:translate-y-0 group-hover:via-transparent group-hover:to-transparent">
                        <p className="text-sm font-bold">{props.data?.title}</p>
                        <div className="pb-2 text-sm">
                            <p>
                                Bạn đã đọc {props.data?.progress}/{props.data?.totalChapters} chương
                            </p>
                        </div>
                    </div>
                    <div className="px-2">
                        <p className="mb-1 flex items-center gap-2">
                            <span className="flex items-center gap-2">
                                <span
                                    className={cx(
                                        "h-2 w-2 rounded-full",
                                        STATUS_CONFIG_2[props.data?.status ?? 1]?.color,
                                    )}
                                />
                                <span className="text-sm font-medium">
                                    {STATUS_CONFIG_2[props.data?.status ?? 1]?.label}
                                </span>
                            </span>
                        </p>
                    </div>
                </div>

                <Progress
                    value={(props.data?.progress / props.data?.totalChapters) * 100}
                    className="absolute bottom-0 left-0 right-0 h-1 bg-neutral-700"
                    indicatorClassName="bg-blue-200"
                />
            </div>
        </Link>
    );
}
