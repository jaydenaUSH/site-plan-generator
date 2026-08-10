import { Link } from 'react-router-dom'
function Sidebar() {
    return (
        <div className="flex w-full border-r-1 flex-col bg-primary h-full px-2 text-white py-10">
            <div>
                <Link to='/'>
                    <h2 className= "text-white!">US Hunger</h2>
                </Link>
            </div>
            <div className="py-10 flex-col flex gap-[10%]">
                <Link to='/' >
                    <h3>New Draft</h3>
                </Link>
                <Link to='/Past'>
                    <p>Existing Venues</p>
                </Link>
                <p>Finalized Drafts (soon)</p>

            </div>


        </div>
    )
}

export default Sidebar;