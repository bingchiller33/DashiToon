import React from "react";
import SeriesDashboardScreen from "../../_screens/SeriesDashboard";
import { NextPageProps } from "@/utils/nextjs";

export default function EditSeries_Page(props: NextPageProps) {
    const { params } = props;
    const seriesId = params?.id;
    return <SeriesDashboardScreen seriesId={seriesId} />;
}
