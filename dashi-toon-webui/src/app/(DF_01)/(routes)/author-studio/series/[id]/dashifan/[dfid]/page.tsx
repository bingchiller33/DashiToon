import TierDetailsScreen from "@/app/(DF_01)/_screens/author-studio/TierDetailsScreen";
import { NextPageProps } from "@/utils/nextjs";
import React from "react";

export default function DashiFanTierPage(props: NextPageProps) {
    const { searchParams, params } = props;
    const { id, dfid } = params;
    return <TierDetailsScreen seriesId={id} tierId={dfid} />;
}
