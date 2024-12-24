import React from "react";
import { NextPageProps } from "@/utils/nextjs";
import ManageSubscriptionScreen from "@/app/(DF_01)/_screens/reader/ManageSubscriptionScreen";

export default function AS_03_Page(props: NextPageProps) {
    const { searchParams, params } = props;
    const { id } = params;
    return <ManageSubscriptionScreen seriesId={id} />;
}
