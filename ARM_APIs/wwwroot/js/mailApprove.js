var armToken = "";
var doTask;
var taskActionCheck = false;
class AxTasksapprove {
    constructor() {
        this.isAxpertFlutter = !this.isNullOrEmpty(armToken);
        this.taskCardHtml = `
        <div class="row">
            <div class="col-md-12 pb-8 task-listing-card" data-process="{{processname}}" data-type="{{tasktype}}" data-taskid="{{taskid}}" data-transid="{{transid}}" data-keyvalue="{{keyvalue}}" >
                <h2 class="task-heading">
                    <input type="checkbox" value="" class="form-check-input task-list-checkbox">
                    <a href="javascript:void(0)" onclick="axTasksObj.openProcess('{{processname}}','{{keyfield}}','{{keyvalue}}','{{taskid}}')" title="{{displaytitle}}">
                        <span class="material-icons material-icons-style material-icons-1 display-icon task-listing-icons">{{displayicon}}</span>
                        {{displaytitle}}
                    </a>
                    
                </h2>
                <div class="task-subtitle">{{displaysubtitle}}</div>
                <div class="mobile-more-btns">
                    <button class="btn btn-icon btn-white btn-color-gray-600 btn-active-primary shadow-sm me-2 tb-btn btn-sm" id="Mobile_more-btns">
                        <span class="material-icons material-icons-style material-icons-2">more_horiz</span>


                    </button><ul class="total-activity">
                        <li>
                            <a href="javascript:void(0)">
                                <span class="material-icons material-icons-style material-icons-2">
                                    <span class="material-symbols-outlined">open_in_new</span>
                                </span><span class="TA-name">Open </span>
                            </a>

                            <a href="javascript:void(0)">
                            <span class="material-icons material-icons-style material-icons-2">
                                <span class="material-symbols-outlined">more_vert</span>
                            </span>
                        </a>

                        </li>
                        <li>
                            <a href="javascript:void(0)">
                                <span class="material-icons material-icons-style material-icons-2" style="color: green;">
                                    <span class="material-symbols-outlined">
                                        check_circle
                                    </span>
                                </span><span class="TA-name">Approve</span>
                            </a>
                            <a href="javascript:void(0)">
                            <span class="material-icons material-icons-style material-icons-2">
                                <span class="material-symbols-outlined">more_vert</span>
                            </span>
                        </a>
                        </li>
                        <li><a href="javascript:void(0)">   <span class="material-icons material-icons-style material-icons-2" style="color: red;">cancel</span><span class="TA-name">Reject</span></a>
                        <a href="javascript:void(0)">
                        <span class="material-icons material-icons-style material-icons-2">
                            <span class="material-symbols-outlined">more_vert</span>
                        </span>
                    </a></li>

                        <li><a href="javascript:void(0)">   <span class="material-icons material-icons-style material-icons-2" style="color: blueviolet;">reply</span><span class="TA-name">Return</span></a>
                        <a href="javascript:void(0)">
                        <span class="material-icons material-icons-style material-icons-2">
                            <span class="material-symbols-outlined">more_vert</span>
                        </span>
                    </a></li>

                        <li>
                            <a href="javascript:void(0)">
                                <span class="material-icons material-icons-style material-icons-2" style="color: brown;">
                                    <span class="material-symbols-outlined">
                                        fast_forward
                                    </span>
                                </span><span class="TA-name">Forward</span>
                            </a>
                            <a href="javascript:void(0)">
                            <span class="material-icons material-icons-style material-icons-2">
                                <span class="material-symbols-outlined">more_vert</span>
                            </span>
                        </a>
                        </li>
                    </ul>
                </div>

                <div class="d-flex flex-row task-date  ">
                    <span title="Assigned From" class="task-assignedBy"> {{fromuser}}</span>
                    <span title="Assigned On" class="start-date"> {{eventdatetime}}</span>
                </div>

                <div class="d-flex flex-row Task-content">
                    <div class="fs-6 fw-semibold  py-2 col-md-12">
                        {{displaycontent}}
                    </div>
                </div>

                
                <div class="text-end task-more-btns">
                    <div class="d-flex align-items-center  task-more-btns">
                        {{task_buttons_html}}
                    </div>
                </div>
            </div>
        </div>`;
        this.taskListHtml = `
        <table class="table align-middle  fs-6 gy-5 mb-0 dataTable no-footer task-listing-card" data-process="{{processname}}" data-type="{{tasktype}}" data-taskid="{{taskid}}"  data-transid="{{transid}}" data-keyvalue="{{keyvalue}}">
            <tbody>
                <tr class="d-block">

                    <td class="d-flex align-items-center task-name border-0">
                        <div class="d-flex flex-column">
                            <span class="List_view accordion-icon arrow{{taskid}}" onclick="axTasksObj.toggleTask('{{taskid}}')">
                                <span class="material-icons material-icons-style material-icons-2">chevron_right</span>
                            </span>
                            <input type="checkbox" value="" class="form-check-input task-list-checkbox">
                            <a href="javascript:void(0)" class="text-gray-800 fw-bold fs-6  mb-1 task-title" title="{{displaytitle}}"><span href="javascript:void(0)" onclick="axTasksObj.openProcess('{{processname}}','{{keyfield}}','{{keyvalue}}','{{taskid}}')">
                            <div class="symbol symbol-50px">
                <span class="symbol-label spanimg-background">
                    <img alt="{{processname}}" class=" w-30px" src="../../custompages/icons/{{processname}}.png"
                        onerror="this.onerror=null;this.src='../../custompages/icons/default.png';">

                </span>
            </div>
                           <span>{{displaytitle}}</span>
                            </a>
                            <span class="task-subtitle">{{displaysubtitle}}</span>
                        </div>
                    </td>

                    <td class="d-flex align-items-center MObile-task-more-btns d-none">
                        <div class="mobile-more-btns">
                            <button class="btn btn-icon btn-white btn-color-gray-600 btn-active-primary shadow-sm me-2 tb-btn btn-sm" id="Mobile_more-btns">
                                <span class="material-icons material-icons-style material-icons-2">more_horiz</span>
                                                      </button>
                            <ul class="total-activity">
                                {{mobile_task_buttons_html}}
                            </ul>
                        </div>
                    </td>

                    <td class="task-right border-0 userDetails">

                        <div class="d-flex flex-row task-process ">
                            <a href="javascript:void(0)" class="text-gray-800 task-assignedBy" title="Assigned By"> {{fromuser}}
                            </a>
                            <a href="javascript:void(0)" title="Assigned On" class="text-gray-800 task-date"> {{eventdatetime}}</a>
                        </div>
                    </td>

                    
                    <td class="task-content border-0 ">

                        <div class="d-flex flex-row task-process">
                       <span class='task-content-span'> {{displaycontent}} </span>
                        </div>
                    </td>
                </tr>
                <tr class="taskSubrow{{taskid}} d-none" data-displaycontent="{{displaycontent}}">
                    <td class="task-description">
                        <span class="more">
                            
                        </span>
                    </td>
                </tr>  
                
                <tr>
                    <td class="text-end task-more-btns">
                        <div class="d-flex align-items-center task-more-btns">{{task_buttons_html}}
                        </div>
                    </td>
                </tr>

            </tbody>
        </table>`

        this.taskNoRecordsCardHtml = `
        <table class="table align-middle  fs-6 gy-5 mb-0 dataTable no-footer task-listing-card">
            <tbody>
                <tr class="d-flex header">                                           
                    <td class="d-flex align-items-center task-name">
                        No active tasks available.
                    </td>
                </tr>
            </tbody>
        </table>`;
        this.taskNotFoundHtml = `
        <table class="table align-middle  fs-6 gy-5 mb-0 dataTable no-footer task-listing-card">
            <tbody>
                <tr class="d-flex header">                                           
                    <td class="d-flex align-items-center task-name">
                        Task details not found.
                    </td>
                </tr>
            </tbody>
        </table>`;
        this.view = "List";
        this.filters = {};
        this.filters.allTasksHtml = `<a href="javascript:void(0)" class="task_activity-counts task-process-selection active" >
                                    <span class="task_activity-name  d-block ">All Tasks</span>
                                    <div class="symbol-label" id="All-tasks-count">...</div>
                                </a>`;
        this.filters.processFilterCardHtml = ` 
        <div class="accordion accordion-icon-toggle task-process-selected" id="" data-process="{{processname}}">
            <div class="accordion-header py-3 d-flex collapsed sub-task-lists-wrap"
                    data-bs-toggle="collapse" data-bs-target="#Task_list-{{processid}}"
                    aria-expanded="false">
                <span class="accordion-icon">
                    <span class="material-icons material-icons-style material-icons-2">chevron_right</span>
                </span>
                <div class="  task_activity_content">
                    <div class="d-flex justify-content-center  " onclick="axTasksObj.filterTasks('process','{{processname}}',this)">
                        <div class="symbol symbol-40px symbol-circle me-5">
                            <div class="symbol-label bgs{{sno}}">
                                <span class="material-icons material-icons-style material-icons-2">account_tree</span>
                            </div>
                        </div>
                        <div class="task-process-selection">
                            <span class="task_activity-name  d-block ">{{processname}}</span>
                            <a href="javascript:void(0)" class="task_activity-counts">
                                <div class="symbol-label">{{processcount}}</div>
                            </a>
                        </div>
                    </div>
                </div>
            </div>
            <div id="Task_list-{{processid}}" class="fs-6 ps-10 accordion-body collapse" data-process="{{processname}}">                
            </div>
        </div>`
        this.filters.typeFilterCardHtml = `
        <div class="step sub-task-lists " onclick="axTasksObj.filterTasks('type','{{processname}}~~{{type}}',this)">
            <div> <span
                    class="material-icons material-icons-style material-icons-2">{{icon}}</span>
            </div>
            <div class="title">{{type}} <span class="task-count-sub">{{count}}</span></div>
        </div>`
        this.filters.viewAllBtnHtml = `<!--<button class="task-activity-all btn btn-white" id="viewAllFilters" onclick="axTasksObj.viewAllFilters();" title="View All">View All</button>-->`;

        this.taskBtnsHtml = {};
        this.taskBtnsHtml.make = `
            <a href="javascript:void(0)" title="Make" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  btn-sm  me-2" onclick="axTasksObj.openProcess('{{processname}}','{{keyfield}}','{{keyvalue}}','{{taskid}}')""><span class="material-icons material-icons-style material-icons-2" style="color: orange;">assignment</span></a>`;
        this.taskBtnsHtml.view = `
            <a href="javascript:void(0)" title="View form" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  btn-sm  me-2 d-none" onclick="axTasksObj.openTstruct('{{transid}}','{{recordid}}','{{processname}}','{{keyvalue}}','{{taskname}}')"><span class="material-icons material-icons-style material-icons-2" style="color: darkmagenta;">visibility</span> View form
          
            
            </a>`;
        this.taskBtnsHtml.history = `
            <a href="javascript:void(0)" title="View History" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  btn-sm  me-2 d-none" onclick="axTasksObj.openHistory('{{processname}}','{{keyfield}}','{{keyvalue}}')""><span class="material-icons material-icons-style material-icons-2" style="color: blue;">history</span>View History
         
            </a>`;

        this.taskBtnsHtml.approve = `
        <div class="task-actions-sets d-flex" data-taskname="{{taskname}}" data-processname="{{processname}}">

            <a href="javascript:void(0)" title="Approve"
                class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  btn-sm  me-2 "
                onclick="axTasksObj.doApprove('{{taskid}}', this);" data-confirmMessage='{{cmsg_appcheck}}'><span class="material-icons material-icons-style material-icons-2"
                    style="color: #47BE7D;">check_circle</span> Approve
            </a>

            <a href="javascript:void(0)" title="Approve Reasons/Comments" data-popover="APPROVE{{taskid}}"
                class="btn btn-white btn-color-gray-700d-inline-flex align-items-center  btn-sm  more-comments taskActionBtn"
                data-kt-menu-trigger="click" data-kt-menu-placement="bottom" data-kt-menu-flip="top"  data-confirmMessage='{{cmsg_appcheck}}' data-action='APPROVE' data-taskid="{{taskid}}">
                <span class="comments-icons material-icons material-icons-style material-icons-2">more_vert</span>
            </a>
            <div  class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary fw-bolder w-400px Active_list_popover"
                data-kt-menu="true">
                <div class="card">
                    <div class="card-body py-5z">
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Approve">
                                <label class="form-label col-form-label">Approve reasons</label>
                                <div class="input-group">
                                    <select data-tasktype="Approve" data-taskid="APPROVE{{taskid}}" class="form-select "
                                        data-tasktype="Approve">
                                        <option value="">NA</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Approve">
                                <label class="form-label col-form-label  {{isApprovalComments}}">Comments</label>
                                <div class="input-group">
                                    <textarea data-tasktype="Approve" data-taskid="APPROVE{{taskid}}" class="form-control"
                                        data-tasktype="Approve" {{isApprovalComments}}></textarea>
                                </div>
                            </div>
                        </div>

                        <div class="text-center Approval-OK">
                            <button class="btn btn-sm btn-primary shadow-sm" onclick="return axTasksObj.doApprove('{{taskid}}', this);" data-confirmMessage='{{cmsg_appcheck}}'>Approve</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
`;
        this.taskBtnsHtml.reject = `
        <div class="task-actions-sets d-flex" data-taskname="{{taskname}}" data-processname="{{processname}}">
            <a href="javascript:void(0)" title="Reject"
                class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  me-2  btn-sm"
                onclick="axTasksObj.doReject('{{taskid}}', this)" data-confirmMessage='{{cmsg_reject}}'><span class="material-icons material-icons-style material-icons-2"
                    style="color: red;">cancel</span>Reject</a>
            <a href="javascript:void(0)" title="Reject Reasons/Comments" data-popover="REJECT{{taskid}}"
                class="btn btn-white btn-color-gray-700d-inline-flex align-items-center  btn-sm  more-comments taskActionBtn"
                data-kt-menu-trigger="click" data-kt-menu-placement="bottom" data-kt-menu-flip="top"  data-confirmMessage='{{cmsg_reject}}' data-action='REJECT' data-taskid="{{taskid}}">
                <span class="comments-icons material-icons material-icons-style material-icons-2">more_vert</span>
            </a>
            <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary fw-bolder w-400px Active_list_popover"
                data-kt-menu="true">
                <div class="card">
                    <div class="card-body py-5z">
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Reject">
                                <label class="form-label col-form-label">Reject reasons</label>
                                <div class="input-group">
                                    <select data-tasktype="Reject" data-taskid="REJECT{{taskid}}" class="form-select "
                                        data-tasktype="Reject">
                                        <option value="">NA</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Reject">
                                <label class="form-label col-form-label  {{isRejectComments}}">Comments</label>
                                <div class="input-group">
                                    <textarea data-tasktype="Reject" data-taskid="REJECT{{taskid}}" class="form-control"
                                        data-tasktype="Reject" {{isRejectComments}}></textarea>
                                </div>
                            </div>
                        </div>

                        <div class="text-center Approval-OK">
                            <button class="btn btn-sm btn-primary shadow-sm" data-confirmMessage='{{cmsg_reject}}' onclick="return axTasksObj.doReject('{{taskid}}', this)">Reject</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
`;
        this.taskBtnsHtml.return = `
        <div class="task-actions-sets d-flex" data-taskname="{{taskname}}" data-processname="{{processname}}">
            <a href="javascript:void(0)" title="Return"
                class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  me-2  btn-sm"
                onclick="axTasksObj.doReturn('{{taskid}}', this, '{{tasktype}}')" data-confirmMessage='{{cmsg_return}}'><span class="material-icons material-icons-style material-icons-2"
                    style="color: blueviolet;">reply</span>Return</a>
            <a href="javascript:void(0)" title="Return Reasons/Comments" data-popover="RETURN{{taskid}}"
                class="btn btn-white btn-color-gray-700d-inline-flex align-items-center  btn-sm  more-comments taskActionBtn"
                data-kt-menu-trigger="click" data-kt-menu-placement="bottom" data-kt-menu-flip="top"  data-confirmMessage='{{cmsg_return}}' data-action='RETURN' data-taskid="{{taskid}}">
                <span class="comments-icons material-icons material-icons-style material-icons-2">more_vert</span>
            </a>
            <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary fw-bolder w-400px Active_list_popover"
                data-kt-menu="true">
                <div class="card">
                    <div class="card-body py-5z">
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Return">
                                <label class="form-label col-form-label">Return To</label>
                                <div class="input-group">
                                    <select data-tasktype="Return" data-taskid="returnto_{{taskid}}" class="form-select " data-tasktype="Return">
                                        <option value="0">Initiator</option>
                                        <option value="1">Previous Level</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Return">
                                <label class="form-label col-form-label">Return reasons</label>
                                <div class="input-group">
                                    <select data-tasktype="Return" data-taskid="RETURN{{taskid}}" class="form-select "
                                        data-tasktype="Return">
                                        <option value="">NA</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Return">
                                <label class="form-label col-form-label  {{isApprovalComments}}">Comments</label>
                                <div class="input-group">
                                    <textarea data-tasktype="Return" data-taskid="RETURN{{taskid}}" class="form-control"
                                        data-tasktype="Return" {{isReturnComments}}></textarea>
                                </div>
                            </div>
                        </div>

                        <div class="text-center Approval-OK">
                            <button class="btn btn-sm btn-primary shadow-sm" data-confirmMessage='{{cmsg_return}}' onclick="return axTasksObj.doReturn('{{taskid}}', this, '{{tasktype}}')">Return</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>`;

        this.taskBtnsHtml.check = `
             <a href="javascript:void(0)" title="Check" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  me-2  btn-sm" onclick="axTasksObj.doCheck('{{taskid}}')">
			 <span class="material-icons material-icons-style material-icons-2" style="color: #47BE7D;">done_all</span>Check
             </a>            
        `;
        this.taskBtnsHtml.more = `
            <button href="javascript:void(0)" class="btn btn-icon btn-active-light-primary w-30px h-30px"
                data-kt-menu-trigger="{default:'click', lg: 'hover'}" data-kt-menu-attach="parent"
                data-kt-menu-placement="bottom-end">
                <span class="material-icons material-icons-style material-icons-1">
                    more_horiz
                </span>
            </button>
            <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-title-gray-700 menu-icon-muted menu-active-bg menu-state-primary fw-bold py-4 fs-6 w-150px Active_list_popover"
                data-kt-menu="true" style="">

                <div class="menu-item px-3">
                    <a href="javascript:void(0)" class="menu-link d-flex px-5 ">
                        <span class="symbol symbol-20px me-4">
                            <span class="material-icons material-icons-style material-icons-1">
                                ios_share
                            </span>
                        </span>Open</a>
                </div>

                <div class="menu-item px-3">
                    <a href="javascript:void(0)" class="menu-link d-flex px-5">
                        <span class="symbol symbol-20px me-4">
                            <span class="material-icons material-icons-style material-icons-1">
                                check_circle
                            </span>
                        </span>Approve</a>
                </div>

                <div class="menu-item px-3">
                    <a href="javascript:void(0)" class="menu-link d-flex px-5">
                        <span class="symbol symbol-20px me-4">
                            <span class="material-icons material-icons-style material-icons-1">
                                cancel
                            </span>
                        </span>Reject </a>
                </div>

                <div class="menu-item px-3">
                    <a href="javascript:void(0)" class="menu-link d-flex px-5">
                        <span class="symbol symbol-20px me-4">
                            <span class="material-icons material-icons-style material-icons-1">
                                reply
                            </span>
                        </span>Return </a>
                </div>
            </div>`;
        this.mobileTaskButtons = {};
        this.mobileTaskButtons.approve = `
        <li>
            <a href="javascript:void(0)" href="javascript:void(0)" title="Approve" data-confirmMessage='{{cmsg_appcheck}}' onclick="axTasksObj.doApprove('{{taskid}}', this);" class="mobile">
                <span class="material-icons material-icons-style material-icons-2" style="color: #47BE7D;">
                    <span class="material-symbols-outlined">
                        check_circle
                    </span>
                </span><span class="TA-name">Approve</span>
            </a>
            <a href="javascript:void(0)" title="Approve Reasons/Comments" data-popover="APPROVE{{taskid}}" data-kt-menu-trigger="click"
                data-kt-menu-placement="bottom" data-kt-menu-flip="top" class="mobile">
                <span class="material-icons material-icons-style material-icons-2">
                    <span class="material-symbols-outlined">more_vert</span>
                </span>
            </a>
            <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary fw-bolder w-400px Active_list_popover"
                data-kt-menu="true">
                <div class="card">
                    <div class="card-body py-5z">
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Approve">
                                <label class="form-label col-form-label">Approve reasons</label>
                                <div class="input-group">
                                    <select data-tasktype="Approve" data-taskid="APPROVE{{taskid}}" class="form-select mobile"
                                        data-tasktype="Approve">
                                        <option value="">NA</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Approve">
                                <label class="form-label col-form-label  {{isApprovalComments}}">Comments</label>
                                <div class="input-group">
                                    <textarea data-tasktype="Approve" data-taskid="APPROVE{{taskid}}" class="form-control mobile"
                                        data-tasktype="Approve" {{isApprovalComments}}></textarea>
                                </div>
                            </div>
                        </div>

                        <div class="text-center Approval-OK">
                            <button class="btn btn-sm btn-primary shadow-sm mobile"
                                onclick="return axTasksObj.doApprove('{{taskid}}', this);" data-confirmMessage='{{cmsg_appcheck}}'>Approve</button>
                        </div>
                    </div>
                </div>
            </div>
        </li>`;
        this.mobileTaskButtons.reject = `
        <li>
            <a href="javascript:void(0)" href="javascript:void(0)" title="Reject" data-confirmMessage='{{cmsg_reject}}' onclick="axTasksObj.doReject('{{taskid}}', this);" class="mobile">
                <span class="material-icons material-icons-style material-icons-2" style="color: red;">
                    <span class="material-symbols-outlined">
                        cancel
                    </span>
                </span><span class="TA-name">Reject</span>
            </a>
            <a href="javascript:void(0)" title="Reject Reasons/Comments" data-popover="REJECT{{taskid}}" data-kt-menu-trigger="click"
                data-kt-menu-placement="bottom" data-kt-menu-flip="top" class="mobile">
                <span class="material-icons material-icons-style material-icons-2">
                    <span class="material-symbols-outlined">more_vert</span>
                </span>
            </a>
            <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary fw-bolder w-400px Active_list_popover"
                data-kt-menu="true">
                <div class="card">
                    <div class="card-body py-5z">
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Reject">
                                <label class="form-label col-form-label">Reject reasons</label>
                                <div class="input-group">
                                    <select data-tasktype="Reject" data-taskid="REJECT{{taskid}}" class="form-select mobile"
                                        data-tasktype="Reject">
                                        <option value="">NA</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Reject">
                                <label class="form-label col-form-label  {{isRejectComments}}">Comments</label>
                                <div class="input-group">
                                    <textarea data-tasktype="Reject" data-taskid="REJECT{{taskid}}" class="form-control mobile"
                                        data-tasktype="Reject" {{isRejectComments}}></textarea>
                                </div>
                            </div>
                        </div>

                        <div class="text-center Reject-OK">
                            <button class="btn btn-sm btn-primary shadow-sm mobile" data-confirmMessage='{{cmsg_reject}}'
                                onclick="return axTasksObj.doReject('{{taskid}}', this);">Reject</button>
                        </div>
                    </div>
                </div>
            </div>
        </li>`;
        this.mobileTaskButtons.return = `
        <li>
            <a href="javascript:void(0)" href="javascript:void(0)" title="Return" data-confirmMessage='{{cmsg_return}}' onclick="axTasksObj.doReturn('{{taskid}}', this, '{{tasktype}}');" class="mobile">
                <span class="material-icons material-icons-style material-icons-2" style="color: blueviolet;">
                    <span class="material-symbols-outlined">
                        reply
                    </span>
                </span><span class="TA-name">Return</span>
            </a>
            <a href="javascript:void(0)" title="Return Reasons/Comments" data-popover="RETURN{{taskid}}" data-kt-menu-trigger="click"
                data-kt-menu-placement="bottom" data-kt-menu-flip="top" class="mobile">
                <span class="material-icons material-icons-style material-icons-2">
                    <span class="material-symbols-outlined">more_vert</span>
                </span>
            </a>
            <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary fw-bolder w-400px Active_list_popover"
                data-kt-menu="true">
                <div class="card">
                    <div class="card-body py-5z">
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Return">
                                <label class="form-label col-form-label">Return To</label>
                                <div class="input-group">
                                    <select data-tasktype="Return" data-taskid="returnto_{{taskid}}" class="form-select mobile" data-tasktype="Return">
                                        <option value="0">Initiator</option>
                                        <option value="1">Previous Level</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Return">
                                <label class="form-label col-form-label">Return reasons</label>
                                <div class="input-group">
                                    <select data-tasktype="Return" data-taskid="RETURN{{taskid}}" class="form-select mobile"
                                        data-tasktype="Return">
                                        <option value="">NA</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div class="row my-3">
                            <div class="approval-controls" data-tasktype="Return">
                                <label class="form-label col-form-label  {{isReturnComments}}">Comments</label>
                                <div class="input-group">
                                    <textarea data-tasktype="Return" data-taskid="RETURN{{taskid}}" class="form-control mobile"
                                        data-tasktype="Return" {{isReturnComments}}></textarea>
                                </div>
                            </div>
                        </div>

                        <div class="text-center Return-OK">
                            <button class="btn btn-sm btn-primary shadow-sm mobile" data-confirmMessage='{{cmsg_return}}'
                                onclick="return axTasksObj.doReturn('{{taskid}}', this, '{{tasktype}}');">Return</button>
                        </div>
                    </div>
                </div>
            </div>
        </li>`;
        this.mobileTaskButtons.make = `
        <li>
            <a href="javascript:void(0)" title="Make" onclick="axTasksObj.openProcess('{{processname}}','{{keyfield}}','{{keyvalue}}','{{taskid}}')">
                <span class="material-icons material-icons-style material-icons-2">
                    <span class="material-symbols-outlined">assignment</span>
                </span><span class="TA-name">Make </span>
            </a>

        </li>`
        this.mobileTaskButtons.view = `
        <li class='d-none'>
            <a href="javascript:void(0)" title="View Form" onclick="axTasksObj.openTstruct('{{transid}}','{{recordid}}','{{processname}}','{{keyvalue}}','{{taskname}}')">

                <span class="material-icons material-icons-style material-icons-2">
                    <span class="material-symbols-outlined" style="color: darkmagenta;">visibility</span>
                </span><span class="TA-name">View Form </span>
            </a>

        </li>`
        this.mobileTaskButtons.history = `
        <li class='d-none'>
            <a href="javascript:void(0)" title="View History" onclick="axTasksObj.openHistory('{{processname}}','{{keyfield}}','{{keyvalue}}')"  style="color: blue;">
                <span class="material-icons material-icons-style material-icons-2">
                    <span class="material-symbols-outlined" style="color: blue;">history</span>
                </span><span class="TA-name">View History </span>
            </a>

        </li>`
        this.mobileTaskButtons.check = `
        <li>
            <a href="javascript:void(0)" title="Check" onclick="axTasksObj.doCheck('{{taskid}}')"  style="color: blue;">
                <span class="material-icons material-icons-style material-icons-2">
                    <span class="material-symbols-outlined" style="color: #47BE7D;">done_all</span>
                </span><span class="TA-name">Check </span>
            </a>

        </li>`
        this.dataSources = [];
        this.processName = '';
        this.task = {};
        this.task.keyField = '';
        this.task.keyValue = '';
        this.task.taskName = '';
        this.task.taskType = '';
        this.task.taskId = '';
    }

    fetchActiveTasks(name) {

        // var result = "{\"result\":{\"message\":\"success\",\"data\":[{\"touser\":\"mkapprover\",\"processname\":\"PEGV2 Process\",\"taskname\":\"Approve - Currency\",\"taskid\":\"1727440000008\",\"tasktype\":\"Approve\",\"edatetime\":\"20230918112850053\",\"eventdatetime\":\"18/09/023 11:28:13\",\"fromuser\":\"mohan\",\"fromrole\":null,\"displayicon\":\"library_add_check\",\"displaytitle\":\"Approve currency C196\",\"displaymcontent\":\"Approve currency C196\\r\\nApprove currency C196\\r\\n{userscount label datasource totalusers}\\r\\n{userscount label datasource totalusers2}\\r\\n\",\"displaycontent\":\"Approve currency C196\\r\\n\",\"displaybuttons\":null,\"keyfield\":\"currency_code\",\"keyvalue\":\"C196\",\"transid\":\"curre\",\"priorindex\":1.0,\"indexno\":2.0,\"subindexno\":1.0,\"approvereasons\":\"A1,A2,A3\",\"defapptext\":null,\"returnreasons\":\"RET1,RET2,RET3\",\"defrettext\":null,\"rejectreasons\":\"REJ1,REJ2\",\"defregtext\":null,\"recordid\":1727440000006.0,\"approvalcomments\":\"T\",\"rejectcomments\":\"T\",\"returncomments\":\"T\",\"returnable\":\"T\",\"taskstatus\":null,\"statusreason\":null,\"statustext\":null,\"username\":null,\"ispending\":\"\",\"initiator\":\"mohan\",\"initiator_approval\":\"T\",\"displaysubtitle\":null,\"allowsend\":\"Any user\",\"allowsendflg\":\"2\",\"cmsg_appcheck\":\"Approve currency?\",\"cmsg_return\":\"Return currency?\",\"cmsg_reject\":\"Reject currency?\",\"showbuttons\":\"T\"}],\"success\":true}}";
        // let json = JSON.parse(result);
        let resultjson = taskjson;
        // let dataResult = this.dataConvert(resultjson, "ARM");
        let dataResult = resultjson;
        this.dataSources[name] = dataResult;

    }

    // connectToAxpert() {
    //     let _this = this;
    //     let url = "../../api/v1/ARMConnectToAxpert";
    //     let data = {};
    //     this.callAPI(url, data, false, result => {
    //         if (result.success) {
    //             let json = JSON.parse(result.response);
    //             let dataResult = _this.dataConvert(json, "ARM");
    //             if (dataResult?.result?.success == true) {
    //                 sessionStorage["connectedToAxpert"] = true;
    //             }
    //             else {
    //                 showAlertDialog("Error", "Error in connecting to Axpert.");
    //             }
    //         }
    //     });
    // }

    // fetchProcessTask(name) {
    //     // let _this = this;
    //     // let url = "../../aspx/AxPEG.aspx/AxGetProcessTask";
    //     // if (_this.isAxpertFlutter) {
    //     //     url = "../../api/v1/ARMGetProcessTask";
    //     // }
    //     // let _task = this.task;

    //     // let data = { taskId: _task.taskId };
    //     // this.callAPI(url, data, false, result => {
    //         // if (result.success) {
    //             var result="{\"result\":{\"message\":\"success\",\"data\":[{\"touser\":\"mkapprover\",\"processname\":\"PEGV2 Process\",\"taskname\":\"Approve - Currency\",\"taskid\":\"1727440000008\",\"tasktype\":\"Approve\",\"edatetime\":\"20230918112850053\",\"eventdatetime\":\"18/09/023 11:28:13\",\"fromuser\":\"mohan\",\"fromrole\":null,\"displayicon\":\"library_add_check\",\"displaytitle\":\"Approve currency C196\",\"displaymcontent\":\"Approve currency C196\\r\\nApprove currency C196\\r\\n{userscount label datasource totalusers}\\r\\n{userscount label datasource totalusers2}\\r\\n\",\"displaycontent\":\"Approve currency C196\\r\\n\",\"displaybuttons\":null,\"keyfield\":\"currency_code\",\"keyvalue\":\"C196\",\"transid\":\"curre\",\"priorindex\":1.0,\"indexno\":2.0,\"subindexno\":1.0,\"approvereasons\":\"A1,A2,A3\",\"defapptext\":null,\"returnreasons\":\"RET1,RET2,RET3\",\"defrettext\":null,\"rejectreasons\":\"REJ1,REJ2\",\"defregtext\":null,\"recordid\":1727440000006.0,\"approvalcomments\":\"T\",\"rejectcomments\":\"T\",\"returncomments\":\"T\",\"returnable\":\"T\",\"taskstatus\":null,\"statusreason\":null,\"statustext\":null,\"username\":null,\"ispending\":\"\",\"initiator\":\"mohan\",\"initiator_approval\":\"T\",\"displaysubtitle\":null,\"allowsend\":\"Any user\",\"allowsendflg\":\"2\",\"cmsg_appcheck\":\"Approve currency?\",\"cmsg_return\":\"Return currency?\",\"cmsg_reject\":\"Reject currency?\",\"showbuttons\":\"T\"}],\"success\":true}}"
    //             let json = JSON.parse(result.response);
    //             let dataResult = _this.dataConvert(json, "ARM");
    //             _this.dataSources[name] = dataResult;
    //         // }
    //     // });
    // }



    fetchProcessTask(name) {
        // var result = "{\"result\":{\"message\":\"success\",\"data\":[{\"touser\":\"mkapprover\",\"processname\":\"PEGV2 Process\",\"taskname\":\"Approve - Currency\",\"taskid\":\"1727440000008\",\"tasktype\":\"Approve\",\"edatetime\":\"20230918112850053\",\"eventdatetime\":\"18/09/023 11:28:13\",\"fromuser\":\"mohan\",\"fromrole\":null,\"displayicon\":\"library_add_check\",\"displaytitle\":\"Approve currency C196\",\"displaymcontent\":\"Approve currency C196\\r\\nApprove currency C196\\r\\n{userscount label datasource totalusers}\\r\\n{userscount label datasource totalusers2}\\r\\n\",\"displaycontent\":\"Approve currency C196\\r\\n\",\"displaybuttons\":null,\"keyfield\":\"currency_code\",\"keyvalue\":\"C196\",\"transid\":\"curre\",\"priorindex\":1.0,\"indexno\":2.0,\"subindexno\":1.0,\"approvereasons\":\"A1,A2,A3\",\"defapptext\":null,\"returnreasons\":\"RET1,RET2,RET3\",\"defrettext\":null,\"rejectreasons\":\"REJ1,REJ2\",\"defregtext\":null,\"recordid\":1727440000006.0,\"approvalcomments\":\"T\",\"rejectcomments\":\"T\",\"returncomments\":\"T\",\"returnable\":\"T\",\"taskstatus\":null,\"statusreason\":null,\"statustext\":null,\"username\":null,\"ispending\":\"\",\"initiator\":\"mohan\",\"initiator_approval\":\"T\",\"displaysubtitle\":null,\"allowsend\":\"Any user\",\"allowsendflg\":\"2\",\"cmsg_appcheck\":\"Approve currency?\",\"cmsg_return\":\"Return currency?\",\"cmsg_reject\":\"Reject currency?\",\"showbuttons\":\"T\"}],\"success\":true}}";
        // let json = JSON.parse(result);
        // var resultjson = json.result.data;
        var resultjson = taskjson;
        // let dataResult = this.dataConvert(resultjson, "ARM");
        let dataResult = resultjson;
        this.dataSources[name] = dataResult;

    }

    checkComments(type, taskId, elem) {
        let isMobile = (elem?.classList?.contains("mobile") ? ".mobile" : ":not(.mobile)");
        let tempType = type.toLowerCase();
        tempType = (tempType == "approve") ? "approval" : tempType;
        let taskProps = axTasksObj.dataSources.TaskReasons[taskId];
        if (taskProps[`${tempType}comments`] == "T" && document.querySelector(`textarea[data-taskid="${type}${taskId}"]${isMobile}`).value == "") {
            if (!document.querySelector(`a[data-popover="${type}${taskId}"]${isMobile}`)?.classList.contains("show")) {
                setTimeout(function () {
                    document.querySelector(`a[data-popover="${type}${taskId}"]${isMobile}`).click();
                }, 100);
            }

            document.querySelector(`textarea[data-taskid="${type}${taskId}"]${isMobile}`).focus();
            showAlertDialog("Error", "Comments are mandatory.");
            return false;
        }
        return true;
    }
    doApprove(taskId, elem) {
        // let isValidComments = this.checkComments('APPROVE', taskId, elem);
        // if (!isValidComments)
        //     return;

        this.doTaskAction('APPROVE', taskId, 'APPROVE', elem);
        return true;
    }

    doReject(taskId, elem) {
        // let isValidComments = this.checkComments('REJECT', taskId, elem);
        // if (!isValidComments)
        //     return;

        this.doTaskAction('REJECT', taskId, 'APPROVE', elem);
    }

    doReturn(taskId, elem, taskType) {
        // let isValidComments = this.checkComments('RETURN', taskId, elem);
        // if (!isValidComments)
        //     return;

        this.doTaskAction('RETURN', taskId, taskType.toUpperCase(), elem);
    }

    doCheck(taskId) {
        this.doTaskAction('CHECK', taskId, 'CHECK');
    }

    doTaskAction(action, taskId, taskType, elem) {
        let isMobile = (elem?.classList?.contains("mobile") ? ".mobile" : ":not(.mobile)");
        let _this = this;
        // let url = "../v1/ARMMailDoTaskAction";
        let url = "../../api/v1/ARMMailDoTaskAction"
        

        let taskReason = "";
        let taskText = "";
        if (taskType == "APPROVE") {
            taskReason = document.querySelector(`select[data-taskid="${action}${taskId}"]${isMobile}`)?.value || '';
            taskText = document.querySelector(`textarea[data-taskid="${action}${taskId}"]${isMobile}`)?.value || '';
        }
        let toUser = "";
        if (action == "RETURN") {
            toUser = document.querySelector(`select[name="returnto_${taskId}"]${isMobile}`)?.value || '';
        }

        doTask = this.checkConfirmation(this, action, taskId, taskType, elem);

        if (!doTask) {
            return false;
        }

        if (action === 'REJECT' || action === 'RETURN' || action === 'APPROVE') {
            let isValidComments = this.checkComments(action, taskId, elem);
            if (!isValidComments)
                return false;
        }

        ShowDimmer(true);
        let data = { ARMSessionId: armSessionId, TaskId: taskId, TaskType: taskType, Action: action, StatusReason: taskReason, StatusText: taskText };
        this.callAPI(url, data, true, result => {
            ShowDimmer(false);
            if (result.success) {
                let json = JSON.parse(result.response);
                if (!_this.isAxpertFlutter) {
                    json = JSON.parse(json.d);
                }
                if (json.result.success) {
                    this.showSuccess(json.result.message);
                    if (!this.isUndefined(parent.axProcessObj)) {
                        parent.axProcessObj.taskCompleted = true;
                        parent.axProcessObj.refreshProcess(parent.axProcessObj.keyValue);
                    }
                    else {
                        setTimeout(function () {
                            window.location.reload();
                        }, 1000)
                    }
                }
                else {
                    this.catchError(json.result.message);
                    ShowDimmer(false)
                }
            }

            //Custom Hook for After task

            try {
                AxAfterTaskAction(this, result);
            }
            catch (ex) {
            }
        });
    }

    callAPI(url, data, async, callBack) {
        let _this = this;
        var xhr = new XMLHttpRequest();
        xhr.open("POST", url, async);
        xhr.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
        if (_this.isAxpertFlutter) {
            xhr.setRequestHeader('Authorization', `Bearer ${armToken}`);
            data["armSessionId"] = armSessionId;
        }

        xhr.onreadystatechange = function () {
            if (this.readyState == 4) {
                if (this.status == 200) {
                    callBack({ success: true, response: this.responseText });
                }
                else {
                    try {
                        var message = JSON.parse(this.responseText)?.result?.message;
                        _this.catchError(message);
                    }
                    catch {
                        _this.catchError(this.responseText);

                    }
                    callBack({ success: false, response: this.responseText });
                }
            }
        }
        xhr.send(JSON.stringify(data));
    }

    catchError(error) {
        showAlertDialog("error", error);
    };

    showSuccess(message) {
        showAlertDialog("success", message);
    };

    isEmpty(elem) {
        return elem == "";
    };

    isNull(elem) {
        return elem == null;
    };

    isNullOrEmpty(elem) {
        return elem == null || elem == "";
    };

    isUndefined(elem) {
        return typeof elem == "undefined";
    };

    dataConvert(data, type) {
        if (type == "AXPERT") {
            try {
                data = JSON.parse(data.d);
                if (typeof data.result[0].result.row != "undefined") {
                    return data.result[0].result.row;
                }
            }
            catch (error) {
                this.catchError(error.message);
            };

            try {
                if (typeof data.result[0].result != "undefined") {
                    return data.result[0].result;
                }
            }
            catch (error) {
                this.catchError(error.message);
            };
        }
        else if (type == "ARM") {
            try {
                if (!this.isAxpertFlutter)
                    data = JSON.parse(data.d);
                if (data.result.success) {
                    if (!this.isUndefined(data.result.data)) {
                        try {
                            return JSON.parse(data.result.data);
                        } catch (e) {
                            return data.result.data;
                        }
                    }
                }
                else {
                    if (!this.isUndefined(data.result.message)) {
                        this.catchError(data.result.message);
                    }
                }
            }
            catch (error) {
                this.catchError(error.message);
            };
        }

        return data;
    }

    generateFldId() {
        return `fid${Date.now()}${Math.floor(Math.random() * 90000) + 10000}`;
    };

    showTasks() {
        if (!this.dataSources.hasOwnProperty('TaskReasons')) {
            this.dataSources["TaskReasons"] = {};
        }

        if (this.dataSources["Tasks"]?.length == undefined || this.dataSources["Tasks"].length == 0) {
            if (this.isUndefined(parent.axProcessObj)) {
                document.querySelector(`#${this.view}_view`).innerHTML = "";
                document.querySelector(`#${this.view}_view`).insertAdjacentHTML("beforeend", ` ${this.taskNoRecordsCardHtml} `);
            }
            else {
                document.querySelector(`#${this.view}_view`).innerHTML = "";
                document.querySelector(`#${this.view}_view`).insertAdjacentHTML("beforeend", ` ${this.taskNotFoundHtml} `);
            }
        }
        else {
            document.querySelector(`#${this.view}_view`).innerHTML = "";
            this.dataSources["Tasks"].forEach((rowData, idx) => {
                if (!this.isNullOrEmpty(rowData.taskstatus)) {
                    let htmlText = `<div class='d-flex p-5'>{{displaycontent}}</div>
                    <div class='d-flex p-5'><span class="material-icons material-icons-style material-icons-2"
                    style="color: #47BE7D;margin-right:5px;">check_circle</span><span>This task has been <b>{{taskstatus}}</b> by <b>{{username}}</b> on <b>{{eventdatetime}}.</b></span> </div>`;
                    if (!this.isNullOrEmpty(rowData.statusreason)) {
                        htmlText += `<div class='d-flex p-5'><span class="material-icons material-icons-style material-icons-2"
                        style="color: #006CFF; margin-right:5px;">mark_chat_read</span><span>Reason: <b>{{statusreason}}</b></span></div>`;
                    }
                    if (!this.isNullOrEmpty(rowData.statustext)) {
                        htmlText += `<div class='d-flex p-5'><span class="material-icons material-icons-style material-icons-2"
                        style="color: #006CFF; margin-right:5px;">chat</span><span>Comments: <b>{{statustext}}</b></span></div>`;
                    }

                    htmlText = Handlebars.compile(htmlText)(rowData);
                    document.querySelector(`#${this.view}_view`).insertAdjacentHTML("beforeend", ` ${htmlText} `);
                    return;
                }
                else if (!this.isNullOrEmpty(rowData.ispending)) {
                    let htmlText = `<span>This task has been pending with user(s):  <b>{{touser}}</b> from <b>{{eventdatetime}}.</b></span>`;
                    if (!this.isNullOrEmpty(rowData.statusreason)) {
                        htmlText += `<span>Reason: <b>{{statusreason}}</b></span>`;
                    }
                    if (!this.isNullOrEmpty(rowData.statustext)) {
                        htmlText += `<span>Comments: <b>{{statustext}}</b></span>`;
                    }

                    htmlText = Handlebars.compile(htmlText)(rowData);
                    document.querySelector(`#${this.view}_view`).insertAdjacentHTML("beforeend", ` ${htmlText} `);
                    return;
                }

                switch (rowData.tasktype.toUpperCase()) {
                    case "MAKE":
                        rowData.task_buttons_html = this.taskBtnsHtml.make + this.taskBtnsHtml.history;
                        rowData.mobile_task_buttons_html = this.mobileTaskButtons.make + this.mobileTaskButtons.history;
                        break;
                    case "CHECK":
                        rowData.task_buttons_html = this.taskBtnsHtml.check + this.taskBtnsHtml.return + this.taskBtnsHtml.view + this.taskBtnsHtml.history;
                        rowData.mobile_task_buttons_html = this.mobileTaskButtons.check + this.mobileTaskButtons.return + this.mobileTaskButtons.view + this.mobileTaskButtons.history;

                        break;
                    case "APPROVE":
                        rowData.task_buttons_html = this.taskBtnsHtml.approve + this.taskBtnsHtml.reject + (rowData?.returnable == 'T' ? this.taskBtnsHtml.return : "") + this.taskBtnsHtml.view + this.taskBtnsHtml.history;
                        rowData.mobile_task_buttons_html = this.mobileTaskButtons.approve + this.mobileTaskButtons.reject + (rowData?.returnable == 'T' ? this.mobileTaskButtons.return : "") + this.mobileTaskButtons.view + this.mobileTaskButtons.history;
                        break;
                }

                this.dataSources.TaskReasons[rowData.taskid] = { approvalcomments: rowData.approvalcomments, rejectcomments: rowData.rejectcomments, returncomments: rowData.returncomments };

                //rowData.task_buttons_html += this.taskBtnsHtml.view + this.taskBtnsHtml.history;
                //rowData.mobile_task_buttons_html += this.mobileTaskButtons.view + this.mobileTaskButtons.history;

                //rowData.task_buttons_html = `<div class="d-flex align-items-center task-more-btns">${rowData.task_buttons_html}</div>`;
                let htmlText = "";

                if (rowData.tasktype.toUpperCase() == "APPROVE") {
                    //document.querySelector(`[data-taskid="${rowData.taskid}"] .Task-content`).classList.remove('d-none');
                    rowData.isApprovalComments = (rowData.approvalcomments == "T" ? `required` : "");
                    rowData.isRejectComments = (rowData.rejectcomments == "T" ? `required` : "");
                    rowData.isReturnComments = (rowData.returncomments == "T" ? `required` : "");
                }
                else if (rowData.tasktype.toUpperCase() == "CHECK") {
                    rowData.isReturnComments = (rowData.returncomments == "T" ? `required` : "");
                }

                if (this.view == "List") {
                    htmlText = Handlebars.compile(this.taskListHtml.replace("{{task_buttons_html}}", rowData.task_buttons_html).replace("{{mobile_task_buttons_html}}", rowData.mobile_task_buttons_html))(rowData);
                }
                else {
                    htmlText = Handlebars.compile(this.taskCardHtml.replace("{{task_buttons_html}}", rowData.task_buttons_html).replace("{{mobile_task_buttons_html}}", rowData.mobile_task_buttons_html))(rowData);
                }
                document.querySelector(`#${this.view}_view`).insertAdjacentHTML("beforeend", ` ${htmlText} `);

                if (rowData.tasktype.toUpperCase() == "APPROVE") {
                    if (!this.isNullOrEmpty(rowData.approvereasons)) {
                        var approvereasons = rowData.approvereasons.split(',');
                        document.querySelectorAll(`select[data-taskid="APPROVE${rowData.taskid}"]`).forEach((select) => {
                            while (select.options.length > 0) {
                                select.remove(0);
                            }
                            for (var i = 0; i < approvereasons.length; i++) {
                                var opt = document.createElement('option');
                                opt.value = approvereasons[i];
                                opt.innerHTML = approvereasons[i];
                                select.appendChild(opt);
                            }
                        })


                    }
                    else {
                        document.querySelectorAll(`select[data-taskid="APPROVE${rowData.taskid}"]`).forEach((select) => {
                            select.closest('.row').classList.add("d-none");
                        })
                    }

                    if (!this.isNullOrEmpty(rowData.defapptext)) {
                        document.querySelectorAll(`textarea[data-taskid="APPROVE${rowData.taskid}"]`).forEach((textarea) => {
                            textarea.value = rowData.defapptext;
                        });
                    }

                    if (!this.isNullOrEmpty(rowData.rejectreasons)) {
                        var rejectreasons = rowData.rejectreasons.split(',');
                        document.querySelectorAll(`select[data-taskid="REJECT${rowData.taskid}"]`).forEach((select) => {
                            while (select.options.length > 0) {
                                select.remove(0);
                            }
                            for (var i = 0; i < rejectreasons.length; i++) {
                                var opt = document.createElement('option');
                                opt.value = rejectreasons[i];
                                opt.innerHTML = rejectreasons[i];
                                select.appendChild(opt);
                            }
                        });
                    } else {
                        document.querySelectorAll(`select[data-taskid="REJECT${rowData.taskid}"]`).forEach((select) => {
                            select.closest('.row').classList.add("d-none");
                        });
                    }

                    if (!this.isNullOrEmpty(rowData.defregtext)) {
                        document.querySelectorAll(`textarea[data-taskid="REJECT${rowData.taskid}"]`).forEach((textarea) => {
                            textarea.value = rowData.defregtext;
                        })
                    }
                }
                if (rowData.tasktype.toUpperCase() == "APPROVE" || rowData.tasktype.toUpperCase() == "CHECK") {
                    if (!this.isNullOrEmpty(rowData.returnreasons)) {
                        var returnreasons = rowData.returnreasons.split(',');
                        document.querySelectorAll(`select[data-taskid="RETURN${rowData.taskid}"]`).forEach((select) => {
                            while (select.options.length > 0) {
                                select.remove(0);
                            }
                            for (var i = 0; i < returnreasons.length; i++) {
                                var opt = document.createElement('option');
                                opt.value = returnreasons[i];
                                opt.innerHTML = returnreasons[i];
                                select.appendChild(opt);
                            }
                        })
                    } else {
                        document.querySelectorAll(`select[data-taskid="RETURN${rowData.taskid}"]`).forEach((select) => {
                            select.closest('.row').classList.add("d-none");
                        })
                    }

                    if (!this.isNullOrEmpty(rowData.defrettext)) {
                        document.querySelectorAll(`textarea[data-taskid="RETURN${rowData.taskid}"]`).forEach((textarea) => {
                            textarea.value = rowData.defrettext;
                        })
                    }
                }
            });

            if (!this.isUndefined(parent.axProcessObj)) {
                document.querySelectorAll(".List_view").forEach((item) => {
                    item.click();
                });
            }

            this.constructFilters();
            this.setApprovalReasons();
        }

        /*var showChar = 20;  // How many characters are shown by default
        var ellipsesText = "...";
        var moreText = "Show more >";
        var lessText = "Show less";

        document.querySelectorAll('.more').forEach((taskContent) => {
            taskContent.classList.add('hideContent');
            taskContent.insertAdjacentHTML("afterend", ` <span class="moreellipses">${ellipsesText}&nbsp;</span><span class="morecontent">&nbsp;&nbsp;<a href="javascript:void(0)" class="morelink show-more">${moreText}</a></span>`);
        });

        $(".show-more").on("click", function () {
            var $this = $(this);
            var $content = $this.closest('.task-description').find(".more");
            var linkText = $this.text().toUpperCase();

            if (linkText === moreText.toUpperCase()) {
                linkText = lessText;
                $content.toggleClass("hideContent", "showContent", 400);
            } else {
                linkText = moreText;
                $content.toggleClass("showContent", "hideContent", 400);
            };

            $this.text(linkText);
        });*/

        //$('.more').each(function () {
        //    var content = $(this).html();

        //    if (content.length > showChar) {

        //        var c = content.substr(0, showChar);
        //        var h = content.substr(showChar, content.length - showChar);

        //        var html = c + '<span class="moreellipses">' + ellipsesText + '&nbsp;</span><span class="morecontent"><span>' + h + '</span>&nbsp;&nbsp;<a href="javascript:void(0)" class="morelink">' + moreText + '</a></span>';

        //        $(this).html(html);
        //    }

        //});

        //$(".morelink").click(function () {
        //    if ($(this).hasClass("less")) {
        //        $(this).removeClass("less");
        //        $(this).html(moreText);
        //    } else {
        //        $(this).addClass("less");
        //        $(this).html(lessText);
        //    }
        //    $(this).parent().prev().toggle();
        //    $(this).prev().toggle();
        //    return false;
        //});        
    }

    constructFilters() {
        document.querySelector(".all-tasks-container").innerHTML = this.filters.allTasksHtml;
        let btnAll = document.querySelector("#All-tasks-count");
        btnAll.closest('.task-process-selected').classList.add("active")
        if (!this.isUndefined(btnAll) && !this.isNull(btnAll)) {
            btnAll.innerHTML = this.dataSources["Tasks"].length.toString();
        }
        if (this.dataSources["Tasks"].length > 0) {
            var processWiseCounts = axTasksObj.dataSources["Tasks"].reduce((p, c) => {
                var name = c.processname;
                if (!p.hasOwnProperty(name)) {
                    p[name] = 0;
                }
                p[name]++;
                return p;
            }, {});

            document.querySelector("#Active-List-container").innerHTML = "";

            let tempSno = 1;
            for (const [key, value] of Object.entries(processWiseCounts)) {
                let rowData = { processname: key, processid: this.sanitizeElementId(key), processcount: value, sno: tempSno };
                var filterHtml = Handlebars.compile(this.filters.processFilterCardHtml)(rowData);
                document.querySelector("#Active-List-container").insertAdjacentHTML("beforeend", ` ${filterHtml} `);
                if (tempSno == 6)
                    tempSno = 1;
                else
                    tempSno++;
            }

            var processWiseCounts = {}
            axTasksObj.dataSources["Tasks"].forEach((row) => {

                if (!processWiseCounts.hasOwnProperty(row.processname)) {
                    processWiseCounts[row.processname] = {};
                }

                if (!processWiseCounts[row.processname].hasOwnProperty(row.tasktype)) {
                    processWiseCounts[row.processname][row.tasktype] = 0;
                }

                processWiseCounts[row.processname][row.tasktype]++;
            });

            for (const [processname, types] of Object.entries(processWiseCounts)) {
                for (const [type, count] of Object.entries(types)) {
                    let rowData = { type: type, count: count, processname: processname, icon: this.getTaskTypeIcon(type) };
                    var filterHtml = Handlebars.compile(this.filters.typeFilterCardHtml)(rowData);
                    document.querySelector(`.accordion-body[data-process="${processname}"]`).insertAdjacentHTML("beforeend", ` ${filterHtml} `);
                }
            }

            KTMenu.createInstances('[data-kt-menu="true"]')
        }
    }

    sanitizeElementId(id) {
        return id.replace(/[^a-zA-Z0-9-_]/g, '');
    }

    getTaskTypeIcon(taskType) {
        let result = "assignment";
        switch (taskType.toUpperCase()) {
            case "MAKE":
                result = "assignment";
                break;
            case "CHECK":
                result = "done_all";
                break;
            case "APPROVE":
                result = "check_circle";
                break;
            case "REJECT":
                result = "cancel";
                break;
            case "RETURN":
                result = "reply";
                break;
            default:
                result = "assignment";
                break;
        }
        return result;
    }

    isEditableTask(processName, taskName, keyValue) {
        var isEditable = false;
        let _this = this;
        let url = "../../aspx/AxPEG.aspx/AxGetEditableTask";
        let data = { processName: processName, taskName: taskName, keyValue: keyValue };
        this.callAPI(url, data, false, result => {
            if (result.success) {
                let json = JSON.parse(result.response);
                let dataResult = _this.dataConvert(json, "ARM");
                if (dataResult.length > 0) {
                    let rowData = dataResult[0];
                    if (rowData?.editable == 'T')
                        isEditable = true
                }
            }
        });
        return isEditable;
    }


    openTstruct(transId, recordId, processName, keyValue, taskName) {
        let url = "";
        if (this.isAxpertFlutter) {
            this.checkAxpertConnection();
            url = `${axpertUrl}/aspx/AxMain.aspx?authKey=AXPERT-${armSessionId}&pname=t${transId}`;
            if (!this.isNullOrEmpty(recordId) && recordId != "0")
                url += `&params=~act=load~recordid=${recordId}`;
            else {
                showAlertDialog("error", "Form details not available.");
                return;
            }
        }
        else {
            var isEditable = false;
            let isPegEdit = `&ispegedit=${isEditable.toString()}`;

            url = `../../aspx/tstruct.aspx?fromprocess=true&transid=${transId}${isPegEdit}`;
            if (!this.isNullOrEmpty(recordId) && recordId != "0")
                url += `&act=load&recordid=${recordId}`;
            else {
                showAlertDialog("error", "Form details not available.");
                return;
            }
        }

        try {
            let modalObj = {
                "id": `ldb${transId}${recordId}`,
                "url": url
            };
            this.openModal(modalObj);
        } catch (error) {
            showAlertDialog("error", error.message);
        }
    }

    openHistory(processName, keyField, keyValue) {
        let url = "";
        if (this.isAxpertFlutter) {
            this.checkAxpertConnection();
            url = `${axpertUrl}/aspx/AxMain.aspx?authKey=AXPERT-${armSessionId}&pname=ipegtaskh&params=~pkeyvalue=${keyValue}~pprocess=${processName}`;
        }
        else {
            url = `../../aspx/ivtoivload.aspx?ivname=pegtaskh&pkeyvalue=${keyValue}&pprocess=${processName}`;
        }
        try {
            let modalObj = {
                "id": `ldb${keyValue}`,
                "url": url
            };
            this.openModal(modalObj);
        } catch (error) {
            showAlertDialog("error", error.message);
        }
    }

    openModal(modalObj) {
        try {
            modalObj.iFrameModalBody = `<iframe id="${modalObj.id}" name="${modalObj.id}" class="col-12 flex-column-fluid w-100 h-100" src="${modalObj.url}" frameborder="0" allowtransparency="True"></iframe>`

            let myModal = new BSModal(`modal_${modalObj.id}`, "", modalObj.iFrameModalBody,
                (opening) => { ShowDimmer(false); }, (closing) => { }
            );

            myModal.changeSize("fullscreen");
            myModal.hideHeader();
            myModal.hideFooter();
            myModal.showFloatingClose();
        } catch (error) {
            showAlertDialog("error", error.message);
        }
    }

    getUrlParams() {
        const queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);
        this.task.keyField = urlParams.get('keyfield');
        this.task.keyValue = urlParams.get('keyvalue');
        this.processName = urlParams.get('processname');
        this.task.taskType = urlParams.get('tasktype');
        this.task.taskId = urlParams.get('taskid');
        this.task.taskName = urlParams.get('taskname');
    }

    // openProcess(processName, keyField, keyValue, taskId) {
    //     ShowDimmer(true);
    //     if (this.isAxpertFlutter) {
    //         this.checkAxpertConnection();
    //         window.location.href = `${axpertUrl}/aspx/AxMain.aspx?authKey=AXPERT-${armSessionId}&pname=hAxProcessFlow&params=~processname=${processName}~keyfield=${keyField}~keyvalue=${keyValue}`;
    //     }
    //     else {
    //         callParentNew(`LoadIframe(htmlPages.aspx?loadcaption=AxProcessFlow&processname=${processName}&keyfield=${keyField}&keyvalue=${keyValue}&taskid=${taskId})`, "function");
    //     }
    // }

    checkAxpertConnection() {
        if (typeof sessionStorage["connectedToAxpert"] == "undefined") {
            this.connectToAxpert();
        }
    }

    filterTasks(type, value, elem) {
        if (type == "process") {
            document.querySelectorAll(".task-listing-card").forEach((task) => {
                if (task.dataset.process == value) {
                    task.classList.remove('d-none');
                }
                else {
                    task.classList.remove('d-none');
                    task.classList.add('d-none');
                }
            })

            document.querySelectorAll(".task-process-selection.active,.sub-task-lists.active,.task-process-selected.active").forEach((filter) => {
                filter.classList.remove('active');
            })

            //document.querySelectorAll(".sub-task-lists.active").forEach((filter) => {
            //    filter.classList.remove('active');
            //})

            elem.querySelectorAll(".task-process-selection").forEach((title) => {
                title.classList.add('active');
            })

            elem.closest(".task-process-selected").classList.add('active')

        }
        else if (type == "type") {
            let tempProcess = value.split("~~")[0];
            let tempType = value.split("~~")[1];

            document.querySelectorAll(".task-listing-card").forEach((task) => {
                if (task.dataset.type == tempType && task.dataset.process == tempProcess) {
                    task.classList.remove('d-none');
                }
                else {
                    task.classList.remove('d-none');
                    task.classList.add('d-none');
                }
            })
            document.querySelectorAll(".sub-task-lists.active").forEach((filter) => {
                filter.classList.remove('active');
            })
            elem.classList.add('active');
        }
        else if (type == "all") {
            document.querySelectorAll(".task-listing-card").forEach((task) => {
                task.classList.remove('d-none');
                task.classList.remove('active');
            })

            document.querySelectorAll(".task-process-selection.active,.sub-task-lists.active,.task-process-selected.active").forEach((filter) => {
                filter.classList.remove('active');
            })

            //document.querySelectorAll(".sub-task-lists.active").forEach((filter) => {
            //    filter.classList.remove('active');
            //})

            elem.querySelectorAll(".task-process-selection").forEach((title) => {
                title.classList.add('active');
            })

            elem.closest(".task-process-selected").classList.add('active');

            //document.querySelectorAll('.accordion-header:not(.collapsed)').forEach((item) => {
            //    item.classList.add('collapsed');
            //})
        }

    }

    viewAllFilters() {
        let expanded = event.target.classList.contains('expanded');
        if (!expanded) {
            document.querySelector('#Active-List-container').classList.remove('horizontal');
            event.target.setAttribute('title', 'Collapse');
            event.target.innerHTML = 'Collapse';
            event.target.classList.add('expanded');
        }
        else {
            document.querySelector('#Active-List-container').classList.add('horizontal');
            event.target.setAttribute('title', 'View All');
            event.target.innerHTML = 'View All';
            event.target.classList.remove('expanded');
        }
    }

    setApprovalReasons() {
        var radios = document.querySelectorAll(".approval-radio");
        for (var i = 0; i < radios.length; i++) {
            radios[i].addEventListener('change', function () {
                var name = this.getAttribute('name');
                var id = this.getAttribute('id');
                document.querySelectorAll(`.${name}`).forEach((tr) => {
                    //tr.classList.remove('d-none');
                    tr.classList.remove('active');
                    //tr.classList.add('d-none');
                })

                this.closest('.approval-radio-group')?.querySelectorAll(`.btn-toggle`)?.forEach((item) => {
                    item.classList.remove('active');
                })

                if (this.checked) {
                    //document.querySelector(`.${name}[data-tasktype="${this.dataset.tasktype}"]`)?.classList?.add('active');
                    document.querySelectorAll(`.${name} .approval-controls`).forEach((controls) => {
                        if (!controls.classList.contains("d-none")) {
                            controls.classList.add("d-none")
                        }
                    });

                    document.querySelectorAll(`.${name} .approval-controls[data-tasktype="${this.dataset.tasktype}"]`).forEach((controls) => {
                        controls.classList.remove("d-none")
                    });

                    document.querySelector(`[for="${id}"]`).classList.add("active");
                }
            });
        }
    }

    toggleFilters() {
        let filter = document.querySelector(".filter-wrapper");
        if (filter.classList.contains("d-none")) {
            filter.classList.remove("d-none");
        }
        else {
            filter.classList.add("d-none");
        }
    }

    toggleViews(elem) {
        let currentView = localStorage["ActiveListView"];
        if (this.isUndefined(currentView)) {
            currentView = "List";
        }

        if (elem.getAttribute("value") == currentView) {
            return;
        }

        if (currentView == "Card") {
            this.showListView();
        }
        else {
            this.showCardView();
        }
    }

    showViews() {
        let currentView = localStorage["ActiveListView"];

        if (this.isUndefined(currentView) || currentView == "List") {
            this.showListView();
        }
        else {
            this.showCardView();
        }
    }

    showListView() {
        localStorage["ActiveListView"] = "List";
        this.view = "List";
        document.querySelectorAll(".task-view").forEach((view) => {
            view.classList.remove('active');
        });
        document.querySelector("#list-view")?.classList?.add("active");

        if (!document.querySelector("#Card_view").classList.contains('d-none')) {
            document.querySelector("#Card_view").classList.add('d-none');
            document.querySelector("#Card_view").innerHTML = "";
        }
        document.querySelector("#List_view").classList.remove('d-none');

        this.showTasks();
    }

    showCardView() {
        localStorage["ActiveListView"] = "Card";
        this.view = "Card";
        document.querySelectorAll(".task-view").forEach((view) => {
            view.classList.remove('active');
        });
        document.querySelector("#card-view")?.classList?.add("active");

        if (!document.querySelector("#List_view").classList.contains('d-none')) {
            document.querySelector("#List_view").classList.add('d-none');
            document.querySelector("#List_view").innerHTML = "";
        }
        document.querySelector("#Card_view").classList.remove('d-none');
        this.showTasks();
    }

    toggleTask(taskId) {
        document.querySelector(`.arrow${taskId}`).classList.toggle('rotate');
        document.querySelectorAll(`.taskSubrow${taskId}`).forEach((view) => {
            view.classList.toggle('d-none');
            if (!view.classList.contains("data-fetched")) {
                let contentHtml = view.dataset["displaycontent"];
                if (contentHtml.indexOf('{') > -1 || contentHtml.indexOf('[') > -1) {
                    let taskContainer = view.closest("table");
                    let axHtmlObj = new AxHTML();
                    axHtmlObj.parse({ name: taskId, input: view.dataset["displaycontent"], data: {}, pageVariables: { "taskid": taskId, "tasktype": taskContainer.getAttribute("data-type"), "transid": taskContainer.getAttribute("data-transid"), "keyvalue": taskContainer.getAttribute("data-keyvalue") }, container: `.taskSubrow${taskId} span` });
                }
                else {
                    document.querySelector(`.taskSubrow${taskId} span`).insertAdjacentHTML("beforeend", ` ${contentHtml} `)
                }
                view.classList.add('data-fetched');
            }
        });
    }



    checkConfirmation(axTasksObj, action, taskId, taskType, elem) {
        var confirmMessage = elem.getAttribute('data-confirmMessage');
        if (taskActionCheck !== `${action}${taskId}` && confirmMessage !== '' && confirmMessage !== null) {
            var CallListViewCB
            CallListViewCB = $.confirm({
                template: '<div class="jconfirm"><div class="jconfirm-bg jconfirm-bg-h"></div><div class="jconfirm-scrollpane"><div class="jconfirm-row"><div class="jconfirm-cell"><div class="jconfirm-holder"><div class="jc-bs3-container"><div class="jc-bs3-row"><div class="jconfirm-box-container jconfirm-animated custom-jconfirm-box"><div class="jconfirm-box" role="dialog" aria-labelledby="labelled" tabindex="-1"><div class="jconfirm-closeIcon">&times;</div><div class="jconfirm-title-c"><span class="jconfirm-icon-c"></span><span class="jconfirm-title"></span></div><div class="jconfirm-content-pane"><div class="jconfirm-content"></div></div><div class="jconfirm-buttons"></div><div class="jconfirm-clear"></div></div></div></div></div></div></div></div></div></div>',
                title: confirmMessage,
                content: '',
                buttons: {
                    buttonA: {
                        text: "Yes",
                        btnClass: 'btn btn-primary',
                        action: function () {
                            CallListViewCB.close();
                            taskActionCheck = `${action}${taskId}`;
                            ShowDimmer(true);
                            axTasksObj.doTaskAction(action, taskId, taskType, elem);
                        }
                    },
                    buttonB: {
                        text: "No",
                        btnClass: 'btn btn-bg-light btn-color-danger btn-active-light-danger',
                        action: function () {
                            CallListViewCB.close();
                            taskActionCheck = false;
                            ShowDimmer(false);
                        }
                    }
                },
                onClose: function () {
                    ShowDimmer(false);
                }
            });

            return false;
        }
        else {
            return true;
        }
    }

    bindEvents() {
        document.querySelectorAll(".taskActionBtn").forEach((item) => {
            KTMenu.getInstance(item).on("kt.menu.dropdown.show", function (item) {
                let itemData = item.dataset;
                axTasksObj.checkPopoverConfirmation(itemData.action, itemData.taskid, item, itemData.confirmmessage);
            });
        });
    }



    checkPopoverConfirmation(action, taskId, menuItem, confirmMessage) {
        if (taskActionCheck !== `${action}${taskId}` && confirmMessage !== '' && confirmMessage !== null) {
            var CallListViewCB
            CallListViewCB = $.confirm({
                template: '<div class="jconfirm"><div class="jconfirm-bg jconfirm-bg-h"></div><div class="jconfirm-scrollpane"><div class="jconfirm-row"><div class="jconfirm-cell"><div class="jconfirm-holder"><div class="jc-bs3-container"><div class="jc-bs3-row"><div class="jconfirm-box-container jconfirm-animated custom-jconfirm-box"><div class="jconfirm-box" role="dialog" aria-labelledby="labelled" tabindex="-1"><div class="jconfirm-closeIcon">&times;</div><div class="jconfirm-title-c"><span class="jconfirm-icon-c"></span><span class="jconfirm-title"></span></div><div class="jconfirm-content-pane"><div class="jconfirm-content"></div></div><div class="jconfirm-buttons"></div><div class="jconfirm-clear"></div></div></div></div></div></div></div></div></div></div>',
                title: confirmMessage,
                content: '',
                buttons: {
                    buttonA: {
                        text: "Yes",
                        btnClass: 'btn btn-primary',
                        action: function () {
                            CallListViewCB.close();
                            taskActionCheck = `${action}${taskId}`;
                            setTimeout(function () {
                                menuItem.focus();
                                menuItem.click();
                            }, 1000);
                        }
                    },
                    buttonB: {
                        text: "No",
                        btnClass: 'btn btn-bg-light btn-color-danger btn-active-light-danger',
                        action: function () {
                            CallListViewCB.close();
                            KTMenu.getInstance(menuItem)?.hide();
                            taskActionCheck = false;
                            ShowDimmer(false);
                        }
                    }
                },
                onClose: function () {
                    ShowDimmer(false);
                }
            });

            KTMenu.getInstance(menuItem)?.hide();
        }
        else {
            //Do nothing
        }
    }
}

var axTasksObj;
var taskstatus
var armSessionId;
var taskjson;
var armMessage;
var taskdetails;
var armJsondata;
$(document).ready(function () {
    appendElements();
    authenticationCheck();
    titleChange();
});


function titleChange(){
    document.title='Mail Approval'
}
function appendElements() {


    const urlQueryString = window.location.search;
    const urlParamsVal = new URLSearchParams(urlQueryString);
    var userName = urlParamsVal.get('userid');

    var mailAuthentication = `<div id='passwordAuthendication' class="card text-center card text-center d-flex">
    <div class="card-body border-0">
    <div class='d-flex justify-content-center UserNameAppend'><span class='hiText'>Hi</span><span class='userNameText'>${userName}</span></div>
   <div class='d-flex justify-content-center'><span id='defalutText' class='defalutText'>Please Enter Your Password To Proceed</span></div>
   <div class="d-flex justify-content-center input-icon left"><input id="passwordfield" class="m-wrap placeholder-no-fix form-control form-control-solid " tabindex="1" type="password" autocomplete="off" placeholder="Password" name="" title="Password" required=""></div>
    </div>
    <div class="text-muted border-0 d-flex justify-content-center">
    <a href="javascript:void(0)" class="btn btn-lg btn-primary mb-5 VerifyBtn"
    id="passwordVerifyBtn">Next</a>
    </div>
  </div>`
    var activeMailStatus = `<!--begin::Container-->
    <div id="kt_content_container" class="">
        <!--begin::Row-->
    
        <h1 class="Task-page-Title text-dark fw-bolder my-1 fs-4" id="taskTitle"></h1>
        <div class="form-check form-check-custom form-check-success form-check-solid task-select-all">
            <input class="form-check-input" type="checkbox" id="select-all">
            <label class="form-check-label" for="select-all">Select All</label>
        </div>
    
        <div class="d-flex align-items-center Activelist-Tools">
            <div id="" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click"
                data-bs-original-title="Search" class="d-none">
                <button type="submit"
                    class="btn btn-icon btn-white btn-color-gray-600 btn-active-primary shadow-sm me-2 tb-btn btn-sm disabled"
                    id="">
                    <span class="material-icons material-icons-style material-icons-2">search</span>
                </button>
            </div>
            <div id="" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click"
                data-bs-original-title="Filter" class="d-none">
                <button type="button"
                    class="btn btn-icon btn-white btn-color-gray-600 btn-active-primary shadow-sm me-2 tb-btn btn-sm disabled"
                    onclick="axTasksObj.toggleFilters();">
                    <span class="material-icons material-icons-style material-icons-2">filter_alt</span>
                </button>
            </div>
            <div data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click"
                data-bs-original-title="List View" class="d-none">
                <button type="button" value="List" onclick="axTasksObj.toggleViews(this);"
                    class="btn btn-icon btn-white btn-color-gray-600 btn-active-primary shadow-sm me-2 tb-btn btn-sm task-view active"
                    id="list-view">
                    <span class="material-icons material-icons-style material-icons-2">checklist</span>
                </button>
            </div>
            <div data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-dismiss="click"
                data-bs-original-title="Card View" class="d-none">
                <button type="button" value="Card" onclick="axTasksObj.toggleViews(this);"
                    class="btn btn-icon btn-white btn-color-gray-600 btn-active-primary shadow-sm me-2 tb-btn btn-sm task-view"
                    id="card-view">
                    <span class="material-icons material-icons-style material-icons-2">view_stream</span>
                </button>
            </div>
    
    
        </div>
    
    
    
        <div class="row " id="ActiveList-overalldiv">
    
            <!--begin:::Col-->
            <div class="col-xl-3 col-md-3 d-flex flex-column flex-column-fluid vh-100 min-vh-100 d-none"
                id="ActiveTask_Left">
                <div id="AllTask_Header" class="d-flex align-items-center task-process-selected">
                    <div class="d-flex justify-content-center  TotalTask-Wrap">
                        <div class="symbol symbol-40px symbol-circle me-5">
                            <div class="symbol-label bg-dark">
                                <span class="material-icons material-icons-style material-icons-2">receipt_long</span>
                            </div>
                        </div>
                        <div class="all-tasks-container" onclick="axTasksObj.filterTasks('all','',this)">
    
                        </div>
                    </div>
    
                </div>
    
    
                <div class="card card-xl-stretch shadow-sm flex-root h-1px  ">
    
                    <!--begin::Body-->
                    <div class="card-body h-300px ">
    
                        <div id="Active-List-container">
                        </div>
                        <!--end::Body-->
                    </div>
                </div>
                <!--end::Table Widget 1-->
            </div>
            <!--end:::Col-->
            <!--begin:::Col-->
            <div class="col-xl-9  col-md-9 d-flex flex-column flex-column-fluid w-50" id="ActiveTask_Right">
    
                <div class="d-flex flex-row filter-wrapper d-none">
    
                    <div class="col-md-4 pe-5">
                        <label class="form-label col-form-label   required">User Name</label>
                        <div class="input-group">
                            <input type="text" class="form-control">
                        </div>
                    </div>
    
                    <div class="col-md-4 pe-5">
                        <label class="form-label col-form-label   required">From Date</label>
                        <div class="input-group">
                            <input type="text" class="form-control">
                        </div>
                    </div>
                    <div class="col-md-3 pe-5">
                        <label class="form-label col-form-label   required">To Date</label>
                        <div class="input-group">
                            <input type="text" class="form-control">
                        </div>
                    </div>
    
                    <div class=" col-md-1 d-flex align-items-end">
                        <button class="btn btn-sm btn-primary task-submit-btn  " title="Reject">Filter</button>
                    </div>
    
    
                </div>
    
                <!--begin::List Widget 3-->
                <div class="card card-xl-stretch flex-root task-view-container d-none " id="Card_view">
    
                </div>
                <!--end::List Widget 3-->
    
                <div class="card card-xl-stretch flex-root task-view-container approvedMeg" id="List_view">
                </div>
            </div>
        </div>
    </div>
  `
    let activeMailStatusAppend = document.querySelector('#activeMailStatus');
    let mailAuthenticationAppend = document.querySelector('#mailAuthentication');
    activeMailStatusAppend.innerHTML = activeMailStatus;
    mailAuthenticationAppend.innerHTML = mailAuthentication;
    if (taskstatus !== 'pending') {
        let activeMailStatusClasslist = document.querySelector('#activeMailStatus');
        activeMailStatusClasslist.classList.remove('d-none');
        var taskdetailsJson = taskdetails;
        var replacedString = taskdetailsJson.replace(/\n/g, ' ').replace(/\r/g, ' ');
        var taskdetailsJsonResult = JSON.parse(replacedString);
        var taskTitle = document.querySelector('#taskTitle');
        if (taskTitle) {
            taskTitleAppend = taskdetailsJsonResult[0].displaytitle;
            taskTitle.innerHTML = taskTitleAppend
        }
        taskjson = taskdetailsJsonResult;
        showTaskDetails();
        var bodyElement = document.querySelector('body');
        if (bodyElement.classList.contains('bgImgAdd')) {
            bodyElement.classList.remove('bgImgAdd');
        }
    } else {
        var bodyelment = document.querySelector('body').classList.add('bgImgAdd');
        let mailAuthenticationClasslist = document.querySelector('#mailAuthentication');
        mailAuthenticationClasslist.classList.remove('d-none');
    }
}


function authenticationCheck() {

    var passwordVerifyBtn = document.querySelector('#passwordVerifyBtn');
    passwordVerifyBtn.addEventListener('click', function () {
        // ShowDimmer(true);
        var passWordField = document.querySelector('#passwordfield');
        var passWordFieldVal = passWordField.value;
        var passWordFieldMd5Val = MD5(passWordFieldVal);

        if (passWordFieldVal !== '') {
            const queryString = window.location.search;
            const urlParams = new URLSearchParams(queryString);
            var taskId = urlParams.get('taskid');
            var userId = urlParams.get('userid');
            var appName = urlParams.get('appname');
            // var passWord = '827ccb0eea8a706c4c34a16891f84e7b';
            var passWord = passWordFieldMd5Val;
            var action = 'Approve'
            try {
                $.ajax({
                    type: "POST",
                    url: '../../api/v1/ARMMailTaskActionDetails',
                    data: JSON.stringify({ TaskId: taskId, UserId: userId, AppName: appName, Password: passWord, Action: action }),
                    cache: false,
                    async: false,
                    contentType: "application/json;charset=utf-8",
                    dataType: "json",
                    success: function (data) {
                        armJsondata = data;
                        armSessionId = armJsondata.result.ARMSessionId;
                        armMessage = armJsondata.result.message
                        armToken = armJsondata.result.token;
                        taskjson = armJsondata.result.taskjson;
                        //ShowDimmer(false);
                        var taskTitle = document.querySelector('#taskTitle');
                        if (taskTitle) {
                            var taskTitleAppend = taskjson[0].displaytitle;
                            taskTitle.innerHTML = taskTitleAppend
                        }
                    },
                    error: function (xhr, status, error) {
                        showAlertDialog('error', 'Please Enter Valid User Credentials');
                        var shortMessageWrapper = document.querySelector('.shortMessageWrapper');
                        setTimeout(function () {
                            shortMessageWrapper.remove();
                        }, 2000);
                        //ShowDimmer(false);
                        return;
                    }

                });
            } catch (e) {
                showAlertDialog("error", e);
                return;
            }
            var activeMailStatusClasslist = document.querySelector('#activeMailStatus');
            var mailAuthenticationClasslist = document.querySelector('#mailAuthentication');
            if (armMessage == 'success') {
                activeMailStatusClasslist.classList.remove('d-none');
                mailAuthenticationClasslist.classList.add('d-none');
                var bodyElement = document.querySelector('body');
                if (bodyElement.classList.contains('bgImgAdd')) {
                    bodyElement.classList.remove('bgImgAdd');
                }
                showTaskDetails();
            } else {
                activeMailStatusClasslist.classList.add('d-none');
                mailAuthenticationClasslist.classList.remove('d-none');
                document.querySelector('body').classList.add('bgImgAdd');
            }
        } else {
            showAlertDialog('error', 'Please enter Password');
            var shortMessageWrapper = document.querySelector('.shortMessageWrapper');
            setTimeout(function () {
                shortMessageWrapper.remove();
            }, 3000);
        }

    });

    document.addEventListener('keydown', (event) => {
        if (event.key === "Enter") {
            passwordVerifyBtn.click();
        }
    });
}

function showTaskDetails() {
    axTasksObj = new AxTasksapprove();
    axTasksObj.getUrlParams();
    axTasksObj.fetchProcessTask("Tasks");

    let _tasks = axTasksObj.task;
    if (axTasksObj.isNullOrEmpty(_tasks.taskName) || axTasksObj.isUndefined(_tasks.taskName)) {
        axTasksObj.fetchActiveTasks("Tasks");
        document.querySelector("#ActiveTask_Left").classList.add('d-none')
    }
    else {
        document.querySelector("#ActiveTask_Left").classList.remove('d-flex');
        document.querySelector("#ActiveTask_Left").style.display = "none";
        axTasksObj.fetchProcessTask("Tasks");
        if (!axTasksObj.isUndefined(axTasksObj.dataSources?.["Tasks"]?.[0]?.["displaytitle"])) {
            document.querySelector("#task-page-title").innerHTML = axTasksObj.dataSources["Tasks"][0]["displaytitle"];
        }
    }
    axTasksObj.showViews();
    axTasksObj.bindEvents();
    if (typeof parent.axProcessObj != "undefined") {
        parent.ShowDimmer(false);
        document.querySelectorAll(".task-title > span").forEach((item) => {
            item.onclick = "#";
        })
    }

    $("#AllTasks").click(function () {
        var filterBox = document.querySelector(".task-activity-container");
        if (filterBox.classList.contains('d-none')) {
            filterBox.classList.remove('d-none');
        }
        else {
            filterBox.classList.add('d-none');
        }

        document.getElementById('task-page-title').click();
    });
}



function closeParentFrame() {
    try {
        eval();
    } catch (ex) {
    }
}

function ShowDimmer(status) {

    var dv = $("#waitDiv");

    if (dv.length > 0 && dv != undefined) {
        if (status == true) {
            closeParentFrame();
            $("body").addClass("page-loading");
            document.onkeydown = function EatKeyPress() {
                return false;
            }
        } else {
            $("body").removeClass("page-loading");
            document.onkeydown = function EatKeyPress() {
                return true;
            }
        }
    } else {

        if (window.opener != undefined) {

            dv = $("#waitDiv", window.opener.document);
            if (dv.length > 0) {
                if (status == true) {
                    $("body", window.opener.document).addClass("page-loading");
                } else {
                    $("body", window.opener.document).removeClass("page-loading");
                }
            }
        }
    }
}



function callParentNew() {
    var e = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : ""
        , t = arguments.length > 1 ? arguments[1] : undefined
        , n = window
        , a = !1;
    "=" === (e = e.trim()).substr(e.length - 1) && (e = (e = e.slice(0, -1)).trim(),
        a = !0);
    try {
        "undefined" == typeof window[e] && (n = window.parent)
    } catch (ex) {
        n = window.parent
    }
    for (var o = !1, i = 0; i < 10; i++)
        try {
            if (!o && checkForCntPlcHldrFrameNew(n))
                o = !0;
            else if (o)
                break;
            if ("id" == t || "class" == t) {
                var r = "id" == t ? "ById" : "sByClassName"
                    , l = n.document["getElement" + r](e);
                if (null == l) {
                    n = n.parent;
                    continue
                }
                return l
            }
            if ("function" == t)
                var c = e.substr(e.indexOf("(") + 1, e.lastIndexOf(")")).slice(0, -1)
                    , s = e.substr(0, e.indexOf("("));
            else
                s = "getParent" == t ? e || "leftMenuWrapper" : e;
            if ("undefined" == typeof n[s]) {
                n = n.parent;
                continue
            }
            return "function" == t ? n[s].apply(this, c.split(",")) : "getParent" === t ? n : a ? (n[e] = t,
                !0) : n[e]
        } catch (ex) {
            n = n.parent;
            continue
        }
}






