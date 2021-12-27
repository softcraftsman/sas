import React from "react"
import MaterialTable from "material-table"
import "./DirectoriesTable.css"

const columns = [
    {
        title: "Space Name",
        field: "name"
    },
    {
        title: "Space Used",
        field: "spaceUsed"
    },
    {
        title: "Monthly Cost",
        field: "monthlyCost"
    },
    {
        title: "Who Has Access",
        field: "members"
    },
    {
        title: "Storage Type",
        field: "storageType"
    }
]

export const DirectoriesTable = ({ data }) => {
    return (
        <div className="directoriesTable">
            <MaterialTable
                title="Your containers"
                columns={columns}
                data={data}
                options={{ search: true, paging: false, filtering: true, exportButton: true }}
            />
        </div>
    )
}

DirectoriesTable.defaultProps = {
    data: [
        { name: "One", spaceUse: "580 MB", monthlyCost: "$ 8.94", members: "Fred Gohsman, Fabricio Sanchez, John Brown", storageType: "Hot" },
        { name: "Two", spaceUse: "1024 KB", monthlyCost: "$ 1.25", members: "Fred Gohsman", storageType: "Cold" }
    ]
}

export default DirectoriesTable
