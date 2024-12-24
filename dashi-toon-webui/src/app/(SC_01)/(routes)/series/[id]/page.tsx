import React from "react";
import SeriesDetailScreen from "@/app/(SC_01)/_screens/SeriesDetailScreen";
import { NextPageProps } from "@/utils/nextjs";

export default function AS_03_Page(props: NextPageProps) {
    const { searchParams, params } = props;
    const { id } = params;
    return (
        <SeriesDetailScreen
            seriesId={id}
            successPayment={searchParams.successPayment === "true"}
            tab={searchParams.tab}
            tierId={searchParams.tierId}
        />
    );
}
