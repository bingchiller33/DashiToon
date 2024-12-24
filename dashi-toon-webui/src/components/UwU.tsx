"use client";
import useUwU from "@/hooks/useUwU";
import React from "react";

export default function UwU(props: {
    children?: React.ReactNode;
    gate?: string;
}) {
    const [uwu] = useUwU(props.gate ?? "uwu");
    if (uwu) {
        return <>{props.children}</>;
    } else {
        return <></>;
    }
}
