"use client";

import { useEffect, useState, useCallback } from "react";
import { useForm } from "react-hook-form";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import AdminSettingLayout from "@/components/AdminSettingLayout";
import SiteLayout from "@/components/SiteLayout";
import { ROLES } from "@/utils/consts";
import { getUsers, getRoles, assignRole, User } from "@/utils/api/admin/user";
import { toast } from "sonner";

export default function UserManagement() {
    const [users, setUsers] = useState<User[]>([]);
    const [roles, setRoles] = useState<string[]>([]);
    const [pagination, setPagination] = useState({
        pageNumber: 1,
        pageSize: 5,
        totalPages: 1,
        totalCount: 5,
        hasNextPage: false,
        hasPreviousPage: false,
    });
    const [selectedUser, setSelectedUser] = useState<User | null>(null);
    const [isAssignRoleOpen, setIsAssignRoleOpen] = useState(false);

    const [filterUserId, setFilterUserId] = useState("");
    const [filterUsername, setFilterUsername] = useState("");
    const [filterRole, setFilterRole] = useState("");

    const fetchUsers = useCallback(async () => {
        const params = {
            pageNumber: pagination.pageNumber,
            pageSize: pagination.pageSize,
            ...(filterUserId && { userId: filterUserId }),
            ...(filterUsername && { username: filterUsername }),
            ...(filterRole && { role: filterRole }),
        };

        const [users, err] = await getUsers(params);
        if (err) {
            toast.error("Không thể tải được danh sách người dùng!");
            return;
        }
        setUsers(users.items);
        setPagination({
            ...pagination,
            totalPages: users.totalPages,
            totalCount: users.totalCount,
            hasNextPage: users.hasNextPage,
            hasPreviousPage: users.hasPreviousPage,
        });
    }, [
        pagination.pageNumber,
        pagination.pageSize,
        filterUserId,
        filterUsername,
        filterRole,
    ]);

    useEffect(() => {
        fetchRoles();
    }, []);

    useEffect(() => {
        fetchUsers();
    }, [fetchUsers, filterUserId, filterUsername, filterRole]);

    const fetchRoles = async () => {
        const [roles, err] = await getRoles();
        if (err) {
            toast.error("Không thể tải các vai trò!");
            return;
        }
        const formattedRoles = roles.map(
            (role) =>
                role.charAt(0).toUpperCase() + role.slice(1).toLowerCase(),
        );
        setRoles(formattedRoles);
    };

    const assignRoleHandle = async (userId: string, role: string) => {
        const [result, err] = await assignRole(userId, role);
        if (err) {
            toast.error("Không thể gán vai trò!");
            return;
        }

        if (!result.succeeded) {
            toast.error(result.errors[0] || "Không thể gán vai trò!");
            return;
        }

        toast.success("Gán vai trò thành công!");
        fetchUsers();
        setIsAssignRoleOpen(false);
    };

    return (
        <SiteLayout allowedRoles={[ROLES.Administrator]} hiddenUntilLoaded>
            <div className="container min-h-screen pt-6">
                <AdminSettingLayout>
                    <h1 className="mb-5 text-2xl font-bold">
                        Quản Lý Người Dùng
                    </h1>

                    <div className="mb-5 flex gap-4">
                        <Input
                            value={filterUserId}
                            onChange={(e) => setFilterUserId(e.target.value)}
                            placeholder="Nhập ID người dùng đầy đủ"
                        />
                        <Input
                            value={filterUsername}
                            onChange={(e) => setFilterUsername(e.target.value)}
                            placeholder="Nhập tên người dùng đầy đủ"
                        />
                        <Select
                            value={filterRole}
                            onValueChange={setFilterRole}
                        >
                            <SelectTrigger className="w-[180px]">
                                <SelectValue placeholder="Lọc" />
                            </SelectTrigger>
                            <SelectContent>
                                {roles.map((role) => (
                                    <SelectItem key={role} value={role}>
                                        {role}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>ID</TableHead>
                                <TableHead>Tên người dùng</TableHead>
                                <TableHead>Email</TableHead>
                                <TableHead>Số điện thoại</TableHead>
                                <TableHead>Vai trò</TableHead>
                                <TableHead>Thao tác</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {users.map((user) => (
                                <TableRow key={user.userId}>
                                    <TableCell>{user.userId}</TableCell>
                                    <TableCell>{user.userName}</TableCell>
                                    <TableCell>{user.email}</TableCell>
                                    <TableCell>{user.phoneNumber}</TableCell>
                                    <TableCell>
                                        {user.roles.join(", ")}
                                    </TableCell>
                                    <TableCell>
                                        <Dialog
                                            open={isAssignRoleOpen}
                                            onOpenChange={setIsAssignRoleOpen}
                                        >
                                            <DialogTrigger asChild>
                                                <Button
                                                    variant="outline"
                                                    onClick={() =>
                                                        setSelectedUser(user)
                                                    }
                                                >
                                                    Gán vai trò
                                                </Button>
                                            </DialogTrigger>
                                            <DialogContent>
                                                <DialogHeader>
                                                    <DialogTitle>
                                                        Gán vai trò cho{" "}
                                                        {selectedUser?.userName}
                                                    </DialogTitle>
                                                </DialogHeader>
                                                <div className="grid gap-4 py-4">
                                                    <div className="grid grid-cols-4 items-center gap-4">
                                                        <Label
                                                            htmlFor="role"
                                                            className="text-right"
                                                        >
                                                            Vai trò
                                                        </Label>
                                                        <Select
                                                            onValueChange={(
                                                                role,
                                                            ) =>
                                                                selectedUser &&
                                                                assignRoleHandle(
                                                                    selectedUser.userId,
                                                                    role,
                                                                )
                                                            }
                                                        >
                                                            <SelectTrigger className="col-span-3">
                                                                <SelectValue placeholder="Chọn vai trò" />
                                                            </SelectTrigger>
                                                            <SelectContent>
                                                                {roles.map(
                                                                    (role) => (
                                                                        <SelectItem
                                                                            key={
                                                                                role
                                                                            }
                                                                            value={
                                                                                role
                                                                            }
                                                                        >
                                                                            {
                                                                                role
                                                                            }
                                                                        </SelectItem>
                                                                    ),
                                                                )}
                                                            </SelectContent>
                                                        </Select>
                                                    </div>
                                                </div>
                                            </DialogContent>
                                        </Dialog>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>

                    {pagination.totalPages > 1 && (
                        <div className="mt-4 flex items-center justify-between">
                            <div>
                                Trang {pagination.pageNumber} /{" "}
                                {pagination.totalPages}
                            </div>
                            <div className="flex gap-2">
                                <Button
                                    onClick={() =>
                                        setPagination((prev) => ({
                                            ...prev,
                                            pageNumber: Math.max(
                                                1,
                                                prev.pageNumber - 1,
                                            ),
                                        }))
                                    }
                                    disabled={pagination.pageNumber === 1}
                                >
                                    Trước
                                </Button>
                                <Button
                                    onClick={() =>
                                        setPagination((prev) => ({
                                            ...prev,
                                            pageNumber: Math.min(
                                                prev.totalPages,
                                                prev.pageNumber + 1,
                                            ),
                                        }))
                                    }
                                    disabled={
                                        pagination.pageNumber ===
                                        pagination.totalPages
                                    }
                                >
                                    Sau
                                </Button>
                            </div>
                        </div>
                    )}
                </AdminSettingLayout>
            </div>
        </SiteLayout>
    );
}
