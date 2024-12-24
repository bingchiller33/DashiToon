import React from "react";
import SetUsernamePage from "../../_screens/SetUsernamePage";
import { NextPageProps } from "@/utils/nextjs";

export default function SetUserPage(props: NextPageProps) {
    const { searchParams } = props;

    const { token, returnUrl } = searchParams;
    return <SetUsernamePage token={token} returnUrl={returnUrl} />;
}
