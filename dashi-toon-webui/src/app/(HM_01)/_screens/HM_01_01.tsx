"use client";

import SiteLayout from "@/components/SiteLayout";
import React, { useRef, useState } from "react";
import Flickity from "react-flickity-component";
import CarouselComponent from "../component/CarouselComponent";
import "./flickity.css";
import UpdatedSeriesTable from "@/components/UpdatedSeriesTable";
import Link from "next/link";
import { BookOpenText, ChevronRight } from "lucide-react";
import {
    getPopularSeries,
    getRecentlyReleasedSeries,
    getRecentlyUpdatedSeries,
    getRecommendedSeries,
    getTrendingGenres,
    getTrendingSeries,
} from "@/utils/api/series";
import Hero from "../component/Hero";
import ListToggle from "@/components/ListToggle";
import { useFetchApi } from "@/hooks/useFetchApi";
import { useLocalStorage } from "usehooks-ts";
import RecommendedSeries from "../component/RecommendedSeries";
import AuthenticatedHomePage from "../component/AuthenticatedHomePage";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { useRouter } from "next/navigation";

const TIMES = [
    {
        name: "Tuần này",
        id: "week",
    },
    {
        name: "Tháng này",
        id: "month",
    },
    {
        id: "year",
        name: "Năm nay",
    },
];
type TimesId = (typeof TIMES)[number]["id"];

export default function HM_01_01Screen() {
    const router = useRouter();
    const [session] = useLocalStorage("session", null);
    const [genres, genresLoading] = useFetchApi(getTrendingGenres, [], {
        onSuccess(data) {
            setChoseGenre(data[0].id.toString());
        },
    });
    const [choseGenre, setChoseGenre] = useState("");
    const [popular, popularLoading] = useFetchApi(getPopularSeries, [choseGenre]);
    const [choseTrending, setChoseTrending] = useState<TimesId>(TIMES[0].id);
    const [trending, trendingLoading] = useFetchApi(getTrendingSeries, [choseTrending]);
    const [released, releasedLoading] = useFetchApi(getRecentlyReleasedSeries);
    const [updated, updatedLoading] = useFetchApi(getRecentlyUpdatedSeries);
    const ref = useRef<HTMLDivElement | null>(null);

    function handleRec() {
        if (!session) {
            toast.error("Vui lòng đăng nhập để xem đề xuất");
            router.push("/login");
            return;
        }

        if (!ref.current) return;
        ref.current?.scrollIntoView?.({ behavior: "smooth" });
    }

    return (
        <SiteLayout
            extras={
                session && (
                    <div ref={ref}>
                        <RecommendedSeries />
                    </div>
                )
            }
        >
            <Hero onClick={handleRec} />
            <div className="container">
                {session && <AuthenticatedHomePage onClick={handleRec} />}
                <CarouselComponent data={trending ?? []} heading="Xu hướng" isLoading={trendingLoading}>
                    <div className="mb-2">
                        <ListToggle
                            items={TIMES}
                            value={choseTrending}
                            onChange={(value) => setChoseTrending(value as TimesId)}
                            labelKey="name"
                            skeletonCount={5}
                        />
                    </div>
                </CarouselComponent>
                <CarouselComponent data={popular ?? []} heading="Thể loại phổ biến" isLoading={popularLoading}>
                    <div className="mb-2">
                        <ListToggle
                            items={genres?.slice(0, 5) ?? []}
                            isLoading={genresLoading}
                            value={choseGenre}
                            onChange={(value) => setChoseGenre(value)}
                            labelKey="name"
                            skeletonCount={5}
                        />
                    </div>
                </CarouselComponent>
                <CarouselComponent data={released ?? []} heading="Mới Xuất Bản" isLoading={releasedLoading} />
                <section className="mt-20">
                    <h2 className="pb-4 text-2xl uppercase">Vừa Cập Nhật</h2>
                    <UpdatedSeriesTable chapters={updated?.slice(0, 12) ?? []} isLoading={updatedLoading} />
                    {updated?.length === 0 && (
                        <div className="flex h-[280px] w-full flex-col items-center justify-center gap-2">
                            <BookOpenText size={48} />
                            <p className="text-muted-foreground">Không có truyện nào, hãy quay lại vào ngày mai!</p>
                        </div>
                    )}
                    <div className="flex justify-end pb-10">
                        <Button
                            variant={"link"}
                            className="flex items-center gap-2 italic text-muted-foreground underline hover:text-blue-400"
                            asChild
                        >
                            <Link href="/recent-updates">
                                Xem tất cả <ChevronRight />
                            </Link>
                        </Button>
                    </div>
                </section>
            </div>
        </SiteLayout>
    );
}
