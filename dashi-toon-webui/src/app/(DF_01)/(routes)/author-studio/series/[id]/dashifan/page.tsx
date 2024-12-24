import React from "react";
import { NextPageProps } from "@/utils/nextjs";
import ManageDashiFanScreen from "@/app/(DF_01)/_screens/author-studio/ManageDashiFanScreen";

export default function AS_03_Page(props: NextPageProps) {
    const { searchParams, params } = props;
    const { id } = params;
    return <ManageDashiFanScreen seriesId={id} />;
}
