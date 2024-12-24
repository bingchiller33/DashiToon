"use client";

import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useEffect, useRef, useState } from "react";

import SiteLayout from "@/components/SiteLayout";
import { Accordion } from "@/components/ui/accordion";
import {
    Analytics,
    getRelatedSeries,
    getSeriesAnalytics,
    getSeriesInfo,
    RelatedSeries,
    SeriesResp,
} from "@/utils/api/series";
import { SERIES_STATUS2 } from "@/utils/consts";
import { toast } from "sonner";
import CongratDashiFanDialog from "../_components/CongratDashiFanDialog";
import DashiFanSection from "../_components/DashiFanSection";
import RelatedSeriesSection from "../_components/RelatedSeriesSection";
import ReviewsSection from "../_components/ReviewsSection";
import SeriesInfo from "../_components/SeriesInfo";
import TableOfContent from "../_components/TableOfContent";
import SeriesAboutSection from "../_components/SeriesAboutSection";

export interface SeriesDetailScreenProps {
    seriesId: string;
    successPayment: boolean;
    tab?: string;
    tierId?: string;
}

export default function SeriesDetailScreen(props: SeriesDetailScreenProps) {
    const tabRef = useRef<any>();
    const { seriesId, successPayment, tab, tierId } = props;
    const [seriesInfo, setSeriesInfo] = useState<SeriesResp>();
    const [analytics, setAnalytics] = useState<Analytics>();
    const [relatedSeries, setRelatedSeries] = useState<RelatedSeries[]>([]);
    useEffect(() => {
        async function work() {
            const [data, err] = await getSeriesInfo(seriesId);
            if (err) {
                console.error("Failed to fetch series info", err);
                toast.error("Failed to fetch series info");
                return;
            }

            setSeriesInfo(data);

            const [analyticsData, analyticsErr] = await getSeriesAnalytics(seriesId);
            if (analyticsErr) {
                console.error("Failed to fetch analytics", analyticsErr);
                return;
            }
            setAnalytics(analyticsData);

            if (tab === "dashifan") {
                tabRef.current?.scrollIntoView({
                    behavior: "smooth",
                    block: "start",
                });
            }
        }

        work();
    }, [seriesId, tab]);

    const refreshAnalytics = async () => {
        const [analyticsData, analyticsErr] = await getSeriesAnalytics(seriesId);
        if (analyticsErr) {
            console.error("Failed to fetch analytics", analyticsErr);
            return;
        }
        setAnalytics(analyticsData);
    };

    useEffect(() => {
        async function work() {
            const [data, err] = await getRelatedSeries(seriesId);
            if (err) {
                console.error("Failed to fetch series info", err);
                toast.error("Failed to fetch series info");
                return;
            }

            setRelatedSeries(data);
        }

        work();
    }, [seriesId]);

    return (
        <SiteLayout>
            <div className="container max-w-6xl py-4">
                <CongratDashiFanDialog seriesId={seriesId} defaultOpen={successPayment} tierId={tierId} />

                <SeriesInfo seriesInfo={seriesInfo} analytics={analytics} />
                <Separator className="mt-4" />
                <Tabs defaultValue={tab ?? "about"} className="mt-4 w-full" ref={tabRef}>
                    <TabsList className="grid w-full grid-cols-4">
                        <TabsTrigger value="about">Về truyện</TabsTrigger>
                        <TabsTrigger value="toc">Mục lục</TabsTrigger>
                        <TabsTrigger value="dashifan">DashiFan</TabsTrigger>
                        <TabsTrigger value="reviews">Đánh giá</TabsTrigger>
                    </TabsList>
                    <TabsContent value="about">
                        <SeriesAboutSection
                            synopsis={seriesInfo?.synopsis}
                            alternativeTitles={seriesInfo?.alternativeTitles}
                        />
                    </TabsContent>
                    <TabsContent value="toc">
                        <Accordion type="multiple">
                            <TableOfContent
                                seriesId={seriesId}
                                seriesType={SERIES_STATUS2[seriesInfo?.type ?? 1].slug}
                            />
                        </Accordion>
                    </TabsContent>
                    <TabsContent value="dashifan">
                        <DashiFanSection seriesId={seriesId} pending={successPayment} />
                    </TabsContent>
                    <TabsContent value="reviews">
                        <ReviewsSection
                            analytics={analytics}
                            seriesId={seriesId}
                            seriesTitle={seriesInfo?.title}
                            onReviewUpdate={refreshAnalytics}
                        />
                    </TabsContent>
                </Tabs>

                <Separator className="mt-4" />
                <RelatedSeriesSection data={relatedSeries} />
            </div>
        </SiteLayout>
    );
}
