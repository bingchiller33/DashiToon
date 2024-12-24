import React from "react";
import { NextPageProps } from "@/utils/nextjs";
import ManageRevenueScreen from "@/app/(AS_04)/_screens/ManageRevenueScreen";

export default function AU_01_Page(props: NextPageProps) {
    const { searchParams, params } = props;
    const { id, vid } = params;

    const pageParams = searchParams["page"];
    const page = pageParams ? parseInt(pageParams) : 1;

    const pageStatus = searchParams["status"];
    const status = pageStatus ? parseInt(pageStatus) : 0;

    return <ManageRevenueScreen />;
}
