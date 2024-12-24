"use client";

import AdminSettingLayout from "@/components/AdminSettingLayout";
import SiteLayout from "@/components/SiteLayout";
import React from "react";
import { DashiFanRatesForm } from "../_components/DashiFanRatesForm";
import { ROLES } from "@/utils/consts";

export default function ManageDashiFanScreen(): React.ReactElement {
    return (
        <SiteLayout allowedRoles={[ROLES.Administrator]} hiddenUntilLoaded>
            <div className="container pt-6">
                <AdminSettingLayout>
                    <div className="space-y-8 p-6 text-neutral-100">
                        <h1 className="mb-6 text-3xl font-bold">
                            Quản lý DashiFan
                        </h1>

                        <section>
                            <DashiFanRatesForm />
                        </section>
                    </div>
                </AdminSettingLayout>
            </div>
        </SiteLayout>
    );
}
