import { Link } from 'react-router-dom'
function Sidebar() {
    return (
        <div className="flex w-full border-r-1 flex-col bg-primary h-full px-2 text-white py-10">
            <div>
                <Link to='/'>
                    <h3>US Hunger</h3>
                </Link>
            </div>
            <div className="py-10 flex-col gap-12">
                <Link to='/Past'>
                    <p>Past</p>
                </Link>
                <p>Finalized Drafts</p>
                <p>SidebarItem</p></div>


        </div>
    )
}

export default Sidebar;