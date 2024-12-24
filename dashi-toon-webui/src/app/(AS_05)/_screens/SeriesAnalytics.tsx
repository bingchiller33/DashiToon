"use client";

import SiteLayout from "@/components/SiteLayout";
import { Button } from "@/components/ui/button";
import { DatePickerWithRange } from "@/components/ui/date-range-picker";
import AnalyticsBreadCrumb from "../_components/AnalyticsBreadCrumb";
import { useState } from "react";
import { DateRange } from "react-day-picker";
import ViewAnalytics from "../_components/ViewAnalytics";
import CommentAnalytics from "../_components/CommentAnalytics";
import ReviewAnalytics from "../_components/ReviewAnalytics";
import KanaAnalytics from "../_components/KanaAnalytics";
import DashiFanAnalytics from "../_components/DashiFanAnalytics";
import GeneralSection from "../_components/GeneralSection";

export interface SeriesAnalyticsProps {
    seriesId: string;
}

export default function SeriesAnalytics(props: SeriesAnalyticsProps) {
    const { seriesId } = props;
    const [current, setCurrent] = useState<DateRange>(() => {
        const end = new Date();
        const start = new Date();
        start.setMonth(end.getMonth() - 1);
        return {
            from: start,
            to: end,
        };
    });

    const [compare, setCompare] = useState<DateRange>();
    return (
        <SiteLayout>
            <div className="pb-4 md:p-4">
                <div className="container mb-4">
                    <AnalyticsBreadCrumb seriesId="1" seriesName="lmao" />
                    <h1 className="text-3xl">Thống kê bộ truyện</h1>
                    <p className="text-muted-foreground">
                        Thống kê chi tiết về các chỉ số quan trọng của bộ truyện
                        giúp bạn hiểu rõ hơn về cộng đồng độc giả của mình
                    </p>
                    <div className="flex flex-wrap items-center justify-end gap-2">
                        <div className="flex flex-wrap items-center justify-end gap-2">
                            <p>Khoảng </p>
                            <DatePickerWithRange
                                value={current}
                                onSelect={(e) => setCurrent(e!)}
                            />
                        </div>
                        <div className="flex flex-wrap items-center justify-end gap-2">
                            <p>So sánh </p>
                            <DatePickerWithRange
                                value={compare}
                                onSelect={(e) => setCompare(e)}
                            />
                            <Button
                                variant={"outline"}
                                onClick={() => setCompare(undefined)}
                            >
                                Hủy so sánh
                            </Button>
                        </div>
                    </div>
                </div>

                <GeneralSection
                    seriesId={seriesId}
                    current={current}
                    compare={compare}
                />

                <section>
                    <div className="container mt-8">
                        <h2 className="text-2xl">Thống kê tương tác</h2>
                        <p className="mb-4 text-muted-foreground">
                            Thống kê chi tiết về sự tương tác của bộ truyện
                        </p>
                    </div>
                    <ViewAnalytics
                        seriesId={seriesId}
                        current={current}
                        compare={compare}
                    />
                    <CommentAnalytics
                        seriesId={seriesId}
                        current={current}
                        compare={compare}
                    />
                    <ReviewAnalytics
                        seriesId={seriesId}
                        current={current}
                        compare={compare}
                    />
                </section>

                <section>
                    <div className="container mt-8">
                        <h2 className="text-2xl">Thống kê thu nhập</h2>
                        <p className="mb-4 text-muted-foreground">
                            Thống kê chi tiết về nguồn thu nhập của bộ truyện
                        </p>
                    </div>
                    <KanaAnalytics
                        seriesId={seriesId}
                        current={current}
                        compare={compare}
                    />
                    <DashiFanAnalytics
                        seriesId={seriesId}
                        current={current}
                        compare={compare}
                    />
                </section>
            </div>
        </SiteLayout>
    );
}
