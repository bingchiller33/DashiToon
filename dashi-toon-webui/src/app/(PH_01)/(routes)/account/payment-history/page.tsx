import React from "react";
import { NextPageProps } from "@/utils/nextjs";
import PaymentHistoryScreen from "@/app/(PH_01)/_screens/PaymentHistoryScreen";

export default function AS_03_Page(props: NextPageProps) {
    const { searchParams, params } = props;
    const { id } = params;
    return <PaymentHistoryScreen />;
}
