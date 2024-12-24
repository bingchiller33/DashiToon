import React from "react";
import { NextPageProps } from "@/utils/nextjs";
import SeriesAnalytics from "@/app/(AS_05)/_screens/SeriesAnalytics";

export default function AU_01_Page(props: NextPageProps) {
    const { params } = props;
    const { id } = params;
    return <SeriesAnalytics seriesId={id} />;
}
