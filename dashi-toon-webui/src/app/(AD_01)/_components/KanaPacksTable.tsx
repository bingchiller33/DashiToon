import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { KanaPackItem } from "@/utils/api/admin";
import { Edit } from "lucide-react";

export interface KanaPacksTableProps {
    data: KanaPackItem[];
    onEdit: (pack: KanaPackItem) => void;
    onActiveChanged: (pack: KanaPackItem, isActive: boolean) => void;
}

export function KanaPacksTable({
    data,
    onActiveChanged,
    onEdit,
}: KanaPacksTableProps): React.ReactElement {
    return (
        <Table>
            <TableHeader>
                <TableRow>
                    <TableHead>ID</TableHead>
                    <TableHead>Số lượng KanaGold</TableHead>
                    <TableHead>Giá (VND)</TableHead>
                    <TableHead>Trạng thái</TableHead>
                    <TableHead>Hành động</TableHead>
                </TableRow>
            </TableHeader>
            <TableBody>
                {data.map((pack) => (
                    <TableRow key={pack.id}>
                        <TableCell>{pack.id}</TableCell>
                        <TableCell>{pack.amount}</TableCell>
                        <TableCell>
                            {pack.price.amount.toLocaleString()}
                        </TableCell>
                        <TableCell>
                            <Switch
                                checked={pack.isActive}
                                onCheckedChange={(e) =>
                                    onActiveChanged(pack, e)
                                }
                            />
                        </TableCell>
                        <TableCell>
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => onEdit(pack)}
                            >
                                <Edit className="mr-2 h-4 w-4" />
                                Chỉnh sửa
                            </Button>
                        </TableCell>
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    );
}
