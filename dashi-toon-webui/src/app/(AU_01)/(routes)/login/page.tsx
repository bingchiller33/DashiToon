import React from "react";
import AU_01_01Screen from "../../_screens/AU_01_01";
import { NextPageProps } from "@/utils/nextjs";

export default function AU_01_Page(props: NextPageProps) {
    const { searchParams } = props;
    return <AU_01_01Screen test={false} returnPath={searchParams.returnUrl??"/"} />;
}
